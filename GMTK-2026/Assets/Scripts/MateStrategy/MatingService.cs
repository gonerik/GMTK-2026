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
        [Inject] private Acid.Factory acidFactory;

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

            if (mate1.GetView().CellSize == CellSize.Small && mate2.GetView().CellSize == CellSize.Acid && mate1.GetMatingEnum() != MatingEnum.Horny)
            {
                mate1.IsMating = true;
                mate2.IsMating = true;
                MatingEnum resultingEnum = mate1.GetMatingEnum() + 1;
                if (mate1.GetMatingEnum() == MatingEnum.Default && mate2.GetMatingEnum() == MatingEnum.Default)
                {
                    CreateAndInitializeCell(resultingEnum, mate1.CellSize, mate1.GetTargetPosition());
                }
                else
                {
                    HandleMutationAndCreation(resultingEnum, mate1.CellSize, mate1.GetTargetPosition(), mate1.GetView().Deviation, mate2.GetView().Deviation);
                }
                mate1.Destroy();
                mate2.Destroy();
                return;
            }
            if(mate2.GetView().CellSize == CellSize.Small && mate1.GetView().CellSize == CellSize.Acid && mate2.GetMatingEnum() != MatingEnum.Horny)
            {
                mate1.IsMating = true;
                mate2.IsMating = true;
                MatingEnum resultingEnum = mate2.GetMatingEnum() + 1;
                if (mate1.GetMatingEnum() == MatingEnum.Default && mate2.GetMatingEnum() == MatingEnum.Default)
                {
                    CreateAndInitializeCell(resultingEnum, mate2.CellSize, mate2.GetTargetPosition());
                }
                else
                {
                    HandleMutationAndCreation(resultingEnum, mate2.CellSize, mate2.GetTargetPosition(), mate1.GetView().Deviation, mate2.GetView().Deviation);
                }
                mate1.Destroy();
                mate2.Destroy();
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

        // Extension seam for Acid variants. Every strain resolves to the one bound Acid prefab today;
        // the planned split - one Acid promoting Default -> Agressive, a second promoting
        // Agressive -> Horny - keys on exactly this strain, because the secreting cell's strain is the
        // strain its Acid should promote. Adding a variant is a case here plus a MatingInstaller binding.
        private void SecreteAcid(MatingEnum strain, Vector3 at)
        {
            int count;
            switch (strain)
            {
                case MatingEnum.Default:
                    count = 1;
                    break;
                case MatingEnum.Agressive:
                    count = 1;
                    break;
                case MatingEnum.Horny:
                    count = 2;
                    break;
                default:
                    return;
            }

            if (count == 1)
            {
                acidFactory.Create().transform.position = at;
                return;
            }

            float randomOffset = UnityEngine.Random.Range(0f, 360f);
            for (int i = 0; i < count; i++)
            {
                float angle = randomOffset + i * (360f / count);
                Vector3 offset = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0);
                acidFactory.Create().transform.position = at + offset;
            }
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