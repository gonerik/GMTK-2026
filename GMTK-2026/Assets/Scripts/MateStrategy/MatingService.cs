using System;
using Cysharp.Threading.Tasks;
using DefaultNamespace;
using Interfaces;
using UnityEngine;
using Zenject;

namespace MateStrategy
{
    public class MatingService
    {
        private MatingConfig matingConfig;
        
        [Inject] private CellUnit.Factory cellFactory;
        [Inject] private RedCell.Factory redCellFactory;
        [Inject] private MatingProbabilities matingProbabilities;
        [Inject] private Acid.AgressiveFactory agressiveAcidFactory;
        // Optional: MatingInstaller skips this binding, with a warning, until the Horny Acid prefab is
        // assigned on MatingInstaller.asset. SpawnAcid tolerates it being null.
        [InjectOptional] private Acid.HornyFactory hornyAcidFactory;

        private int mutationChance;
        private static readonly int HornyAdditionalSpawnChance = 50;
        private const string MutateSoundID = "event:/Cell mutates";
        // Deliberate reuse: GoopCore.fspro has no acid event yet. A separate constant so a dedicated
        // event can be swapped in here without disturbing deviation-mutation audio.
        private const string SecretionSoundID = "event:/Cell mutates";

        [Inject]
        public MatingService(MatingConfig matingConfig, GameObject cellPrefab)
        {
            this.matingConfig = matingConfig;
            StartMutationGrowth().Forget();
        }

        private async UniTaskVoid StartMutationGrowth()
        {
            while (true)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(matingConfig.MutationChanceGrowthRate));
                mutationChance++;
            }
        }

        public void Mate(IMate mate1, IMate mate2)
        {
            if (mate1 == null || mate2 == null)
            {
                return;
            }

            // Either collider can be the Acid, so try both orientations.
            if (TryConsumeAcid(mate1, mate2) || TryConsumeAcid(mate2, mate1))
            {
                return;
            }

            if (mate1.GetView().CellSize != mate2.GetView().CellSize)
            {
                return;
            }

            if (mate1.IsMating || mate2.IsMating)
            {
                return;
            }

            if (mate1.CellSize == CellSize.Large || mate2.CellSize == CellSize.Large)
            {
                TrySecreteFromPairing(mate1, mate2);
                return;
            }
            
            mate1.IsMating = true;
            mate2.IsMating = true;
            
            
            MatingEnum resultEnum = DetermineResultingEnum(mate1.GetMatingEnum(), mate2.GetMatingEnum());
            CellSize newSize = mate1.CellSize + 1;
            Vector3 spawnPos = (mate1.GetTargetPosition() + mate2.GetTargetPosition()) / 2f;

            if (mate1.GetMatingEnum() == MatingEnum.Agressive && mate2.GetMatingEnum() == MatingEnum.Agressive)
            {
                resultEnum = MatingEnum.Horny;
            }

            if (mate1.GetMatingEnum() == MatingEnum.Horny)
            {
                int additionalSpawnProbability = UnityEngine.Random.Range(0, 100);
                if (additionalSpawnProbability <= HornyAdditionalSpawnChance)
                {
                    MatingEnum resultEnumHorny1 = DetermineResultingEnum(mate1.GetMatingEnum(), mate2.GetMatingEnum());
                    Vector3 spawnOffset = UnityEngine.Random.insideUnitCircle * 1f;
                    HandleMutationAndCreation(resultEnumHorny1, mate1.CellSize, spawnPos + spawnOffset, mate1.GetView().Deviation, mate2.GetView().Deviation);
                }
            }
            if(mate2.GetMatingEnum() == MatingEnum.Horny)
            {
                int additionalSpawnProbability = UnityEngine.Random.Range(0, 100);
                if (additionalSpawnProbability <= HornyAdditionalSpawnChance)
                {
                    MatingEnum resultEnumHorny2 = DetermineResultingEnum(mate1.GetMatingEnum(), mate2.GetMatingEnum());
                    Vector3 spawnOffset = UnityEngine.Random.insideUnitCircle * 1f;
                    HandleMutationAndCreation(resultEnumHorny2, mate2.CellSize, spawnPos + spawnOffset, mate1.GetView().Deviation, mate2.GetView().Deviation);
                }
            }

            if (mate1.GetMatingEnum() == MatingEnum.Default && mate2.GetMatingEnum() == MatingEnum.Default)
            {
                CreateAndInitializeCell(resultEnum, newSize, spawnPos);
            }
            else
            {
                HandleMutationAndCreation(resultEnum, newSize, spawnPos, mate1.GetView().Deviation, mate2.GetView().Deviation);
            }
            mate1.Destroy();
            mate2.Destroy();
        }

        // Two Large cells of the same strain that touch secrete Acid, once per cell. This replaces
        // the old "Large cell dies -> drops Acid" hook that used to live in LargeSize.HandleOnDie.
        // Neither cell is destroyed, and IsMating is deliberately left alone: RedCell.OnCollisionEnter2D
        // skips cells whose IsMating is set, so latching it here would make paired cells predator-proof.
        private void TrySecreteFromPairing(IMate mate1, IMate mate2)
        {
            if (!(mate1 is CellUnit cell1) || !(mate2 is CellUnit cell2))
            {
                return;
            }

            if (cell1.CellSize != CellSize.Large || cell2.CellSize != CellSize.Large)
            {
                return;
            }

            if (cell1.MatingEnum != cell2.MatingEnum || cell1.MatingEnum == MatingEnum.Acid)
            {
                return;
            }

            if (cell1.Deviation != DeviationEnum.Default || cell2.Deviation != DeviationEnum.Default)
            {
                return;
            }

            // The only touch that yields nothing is one between two cells that have both already
            // paired. Latching both flags here is also what stops Unity's second OnCollisionEnter2D
            // callback - the one raised on the other collider - from secreting a second time.
            if (cell1.HasPaired && cell2.HasPaired)
            {
                return;
            }

            cell1.MarkPaired();
            cell2.MarkPaired();

            Vector3 spawnPos = (cell1.GetTargetPosition() + cell2.GetTargetPosition()) / 2f;
            SecreteAcid(cell1.MatingEnum, spawnPos);
            FMODUnity.RuntimeManager.PlayOneShot(SecretionSoundID);
        }

        // A Small cell touching an Acid it can consume is promoted to the strain that Acid grants, at the
        // same size, and both are consumed. Any other Small/Acid touch is a no-op for both.
        private bool TryConsumeAcid(IMate cellMate, IMate acidMate)
        {
            if (!(acidMate is Acid acid) || cellMate.CellSize != CellSize.Small)
            {
                return false;
            }

            MatingEnum cellStrain = cellMate.GetMatingEnum();
            if (cellStrain != acid.ConsumesStrain)
            {
                return false;
            }

            cellMate.IsMating = true;
            acid.IsMating = true;

            // Mirrors the mating rule below: promoting an Ordinary cell never rolls for a mutation,
            // promoting anything further along does.
            if (cellStrain == MatingEnum.Default)
            {
                CreateAndInitializeCell(acid.GrantsStrain, cellMate.CellSize, cellMate.GetTargetPosition());
            }
            else
            {
                HandleMutationAndCreation(acid.GrantsStrain, cellMate.CellSize, cellMate.GetTargetPosition(), cellMate.GetView().Deviation, acid.GetView().Deviation);
            }

            cellMate.Destroy();
            acid.Destroy();
            return true;
        }

        // Which Acid a pairing yields is set by the strain that paired. Horny is the terminal strain, so it
        // has no Acid of its own; it yields one of each kind instead.
        private void SecreteAcid(MatingEnum strain, Vector3 at)
        {
            IFactory<Acid>[] acids;
            switch (strain)
            {
                case MatingEnum.Default:
                    acids = new IFactory<Acid>[] { agressiveAcidFactory };
                    break;
                case MatingEnum.Agressive:
                    acids = new IFactory<Acid>[] { hornyAcidFactory };
                    break;
                case MatingEnum.Horny:
                    acids = new IFactory<Acid>[] { agressiveAcidFactory, hornyAcidFactory };
                    break;
                default:
                    return;
            }

            if (acids.Length == 1)
            {
                SpawnAcid(acids[0], at);
                return;
            }

            float randomOffset = UnityEngine.Random.Range(0f, 360f);
            for (int i = 0; i < acids.Length; i++)
            {
                float angle = randomOffset + i * (360f / acids.Length);
                Vector3 offset = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0);
                SpawnAcid(acids[i], at + offset);
            }
        }

        // The Horny factory is null until its prefab is assigned; MatingInstaller already warns about it.
        private static void SpawnAcid(IFactory<Acid> factory, Vector3 at)
        {
            if (factory == null)
            {
                return;
            }

            factory.Create().transform.position = at;
        }

        private void HandleMutationAndCreation(MatingEnum resultEnum, CellSize newSize, Vector3 spawnPos, DeviationEnum parent1Deviation, DeviationEnum parent2Deviation)
        {
            int redProb = matingProbabilities.GetProbability(DeviationEnum.Red);
            int yellowProb = matingProbabilities.GetProbability(DeviationEnum.Yellow);
            int blueProb = matingProbabilities.GetProbability(DeviationEnum.Blue);

            // Inheritance logic
            int inheritanceBonus = 30; // 30% bonus for having a parent with deviation
            
            if (parent1Deviation == DeviationEnum.Red) redProb += inheritanceBonus;
            if (parent2Deviation == DeviationEnum.Red) redProb += inheritanceBonus;
            
            if (parent1Deviation == DeviationEnum.Yellow) yellowProb += inheritanceBonus;
            if (parent2Deviation == DeviationEnum.Yellow) yellowProb += inheritanceBonus;
            
            if (parent1Deviation == DeviationEnum.Blue) blueProb += inheritanceBonus;
            if (parent2Deviation == DeviationEnum.Blue) blueProb += inheritanceBonus;
            
            int totalMutationChance = redProb + yellowProb + blueProb + mutationChance;
            int mutationProbability = UnityEngine.Random.Range(0, 100);

            if (mutationProbability < totalMutationChance)
            {
                FMODUnity.RuntimeManager.PlayOneShot(MutateSoundID);
                
                int selectionRoll = UnityEngine.Random.Range(0, redProb + yellowProb + blueProb);
                if (selectionRoll < blueProb)
                {
                    cellFactory.Create().Initialize(resultEnum, newSize, DeviationEnum.Blue, spawnPos);
                }
                else if (selectionRoll < blueProb + yellowProb)
                {
                    cellFactory.Create().Initialize(resultEnum, newSize, DeviationEnum.Yellow, spawnPos);
                }
                else
                {
                    redCellFactory.Create().Initialize(resultEnum, newSize, spawnPos);
                }
            }
            else
            {
                CreateAndInitializeCell(resultEnum, newSize, spawnPos);
            }
        }

        private void CreateAndInitializeCell(MatingEnum resultEnum, CellSize newSize, Vector3 spawnPos)
        {
            cellFactory.Create().Initialize(resultEnum, newSize,  DeviationEnum.Default, spawnPos);
        }

        private MatingEnum DetermineResultingEnum(MatingEnum parent1, MatingEnum parent2)
        {
            Vector2 probabilityMatrix  = new Vector2();
            probabilityMatrix.x = matingConfig.GetMatingChanceModifier(parent1);
            probabilityMatrix.y = matingConfig.GetMatingChanceModifier(parent2);
            probabilityMatrix.Normalize();
            probabilityMatrix *= 100;
            int randomIndex = UnityEngine.Random.Range(0, 100);
            if (randomIndex <= probabilityMatrix.x)
            {
                return parent1;
            }
            return parent2;
        }
    }
}