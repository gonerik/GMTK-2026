using System;
using Cell;
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
        // assigned on MatingInstaller.asset. SpawnAcid tolerates it being null, so Bursting still works.
        [InjectOptional] private Acid.HornyFactory hornyAcidFactory;
        [Inject] private StrainDiscovery strainDiscovery;

        private int mutationChance = 0;
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
                //mutationChance++;
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

            // Either cell can be the devourer, so try both orientations.
            if (TryDevour(mate1, mate2) || TryDevour(mate2, mate1))
            {
                return;
            }

            if (mate1.GetView().CellSize != mate2.GetView().CellSize)
            {
                return;
            }

            // Mating and pairing happen within a strain, except that a Horny cell also mates with an
            // Ordinary one (ADR-0006). Any other cross-strain touch does nothing.
            MatingEnum strain1 = mate1.GetMatingEnum();
            MatingEnum strain2 = mate2.GetMatingEnum();
            bool hornyMix = (strain1 == MatingEnum.Horny && HornyStrategy.AcceptsPartner(strain2))
                            || (strain2 == MatingEnum.Horny && HornyStrategy.AcceptsPartner(strain1));
            if (strain1 != strain2 && !hornyMix)
            {
                return;
            }

            // Agressive cells neither mate nor pair; they grow by Devouring instead (ADR-0005).
            if (mate1.GetMatingEnum() == MatingEnum.Agressive)
            {
                return;
            }

            if (mate1.IsMating || mate2.IsMating)
            {
                return;
            }

            // A mixed Large pair is rejected inside pairing by its same-strain check; what a Large
            // Horny and a Large Ordinary do together is still to be designed.
            if (mate1.CellSize == CellSize.Large || mate2.CellSize == CellSize.Large)
            {
                TrySecreteFromPairing(mate1, mate2);
                return;
            }

            mate1.IsMating = true;
            mate2.IsMating = true;

            CellSize stage = mate1.CellSize;
            CellSize newSize = stage + 1;
            Vector3 spawnPos = (mate1.GetTargetPosition() + mate2.GetTargetPosition()) / 2f;
            DeviationEnum deviation1 = mate1.GetView().Deviation;
            DeviationEnum deviation2 = mate2.GetView().Deviation;

            if (strain1 == MatingEnum.Default && strain2 == MatingEnum.Default)
            {
                // The Ordinary line never mutates through its own matings.
                CreateAndInitializeCell(MatingEnum.Default, newSize, spawnPos);
            }
            else
            {
                // A Horny parent is involved. The offspring is always Horny, so mating never shifts a strain
                // (only Acid does), and the mating yields a Brood of copies at the parents' stage.
                // In a mixed pair the partner is the Ordinary parent; for Horny + Horny either will do.
                MatingEnum partnerStrain = strain1 == MatingEnum.Horny ? strain2 : strain1;
                HandleMutationAndCreation(MatingEnum.Horny, newSize, spawnPos, deviation1, deviation2);
                SpawnBrood(partnerStrain, stage, spawnPos, deviation1, deviation2);
            }
            mate1.Destroy();
            mate2.Destroy();
        }

        // The extra cells a Horny mating yields besides its offspring: one copy of the partner, sometimes a
        // second, and sometimes a copy of the Horny parent. Rolled once per mating, never per parent. Every
        // copy, an Ordinary one included, rolls for a mutation.
        private void SpawnBrood(MatingEnum partnerStrain, CellSize stage, Vector3 at, DeviationEnum deviation1, DeviationEnum deviation2)
        {
            int partnerCopies = UnityEngine.Random.Range(0, 100) < matingConfig.SecondPartnerCopyChance ? 2 : 1;
            for (int i = 0; i < partnerCopies; i++)
            {
                HandleMutationAndCreation(partnerStrain, stage, at + (Vector3)(UnityEngine.Random.insideUnitCircle * 1f), deviation1, deviation2);
            }

            if (UnityEngine.Random.Range(0, 100) < matingConfig.SelfCopyChance)
            {
                HandleMutationAndCreation(MatingEnum.Horny, stage, at + (Vector3)(UnityEngine.Random.insideUnitCircle * 1f), deviation1, deviation2);
            }
        }

        // Two Large cells of the same strain that touch secrete Acid. This replaces the old "Large cell
        // dies -> drops Acid" hook that used to live in LargeSize.HandleOnDie. Acid should cost cells, so
        // both are destroyed (ADR-0001, amended). Agressive cells never get here: they don't pair.
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

            // Destroy only takes effect at the end of the frame, so Unity's second OnCollisionEnter2D
            // callback - the one raised on the other collider - still arrives. Latching both flags here
            // is what stops it secreting a second time.
            if (cell1.HasPaired && cell2.HasPaired)
            {
                return;
            }

            cell1.MarkPaired();
            cell2.MarkPaired();

            Vector3 spawnPos = (cell1.GetTargetPosition() + cell2.GetTargetPosition()) / 2f;
            SecreteAcid(cell1.MatingEnum, spawnPos);
            mate1.Destroy();
            mate2.Destroy();
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
            // Agressive cells devour and never take Acid (ADR-0005). That leaves Horny Acid, whose recipe
            // still names Agressive, with no consumer until the recipe is revisited.
            if (cellStrain != acid.ConsumesStrain || cellStrain == MatingEnum.Agressive)
            {
                return false;
            }

            cellMate.IsMating = true;
            acid.IsMating = true;
            
            HandleMutationAndCreation(acid.GrantsStrain, cellMate.CellSize, cellMate.GetTargetPosition(), cellMate.GetView().Deviation, acid.GetView().Deviation);
            // Discovers the strain even when the mutation roll above produced a Predator (ADR-0004).
            strainDiscovery.Discover(acid.GrantsStrain);

            cellMate.Destroy();
            acid.Destroy();
            return true;
        }

        // An Agressive cell touching a cell it can devour destroys it, and the prey pays out its energy
        // as any death does. Its Nourishment goes to the devourer; enough of it grows the devourer a stage
        // in place, carrying over the excess, and a Large devourer Bursts instead.
        private bool TryDevour(IMate devourerMate, IMate preyMate)
        {
            if (!(devourerMate is CellUnit devourer) || !(preyMate is CellUnit prey))
            {
                return false;
            }

            if (devourer.IsMating || prey.IsMating || !AgressiveStrategy.CanDevour(devourer, prey.GetView()))
            {
                return false;
            }

            // Latched so Unity's second OnCollisionEnter2D callback for this contact is ignored.
            prey.IsMating = true;
            int nourishment = devourer.Nourishment + matingConfig.GetNourishment(prey.CellSize);
            prey.Destroy();

            int needed = matingConfig.GetNourishmentToGrow(devourer.CellSize);
            if (nourishment < needed)
            {
                devourer.Nourish(nourishment);
            }
            else if (devourer.CellSize == CellSize.Large)
            {
                Burst(devourer);
            }
            else
            {
                // Growth never rolls for a mutation.
                devourer.GrowInPlace(devourer.CellSize + 1, nourishment - needed);
            }
            return true;
        }

        // A fully nourished Large Agressive cell dies and leaves one Horny Acid where it stood. It pays out
        // its energy like any death. Not a Promotion, so it Discovers nothing.
        private void Burst(CellUnit cell)
        {
            cell.IsMating = true;
            SpawnAcid(hornyAcidFactory, cell.GetTargetPosition());
            FMODUnity.RuntimeManager.PlayOneShot(SecretionSoundID);
            cell.Destroy();
        }

        // Which Acid a pairing yields is set by the strain that paired. Agressive cells never pair; Horny
        // is the terminal strain, so it has no Acid of its own and yields one of each kind instead.
        private void SecreteAcid(MatingEnum strain, Vector3 at)
        {
            IFactory<Acid>[] acids;
            switch (strain)
            {
                case MatingEnum.Default:
                    acids = new IFactory<Acid>[] { agressiveAcidFactory };
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
            
            // if (parent1Deviation == DeviationEnum.Red) redProb += inheritanceBonus;
            // if (parent2Deviation == DeviationEnum.Red) redProb += inheritanceBonus;
            //
            // if (parent1Deviation == DeviationEnum.Yellow) yellowProb += inheritanceBonus;
            // if (parent2Deviation == DeviationEnum.Yellow) yellowProb += inheritanceBonus;
            //
            // if (parent1Deviation == DeviationEnum.Blue) blueProb += inheritanceBonus;
            // if (parent2Deviation == DeviationEnum.Blue) blueProb += inheritanceBonus;
            
            int totalMutationChance = redProb + yellowProb + blueProb + mutationChance;
            int mutationProbability = UnityEngine.Random.Range(0, 100);
            Debug.Log(totalMutationChance + "           " + mutationProbability);

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
    }
}