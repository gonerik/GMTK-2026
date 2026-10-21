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
        [Inject] private CellVisualConfig cellVisualConfig;

        private int mutationChance = 0;
        // Brood copies that keep failing to appear grow likelier (see RollGrowingChance). One count per roll,
        // shared by every Horny mating for the whole run.
        private int secondPartnerCopyMisses;
        private int selfCopyMisses;
        // Brood copies land anywhere within this distance of the mating point.
        private const float BroodScatter = 1f;
        private const string WallLayer = "PetriDish";
        private readonly RaycastHit2D[] wallHits = new RaycastHit2D[1];
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

            // Mating and pairing happen within a strain, except that a Horny cell also mates and pairs with
            // an Ordinary one (ADR-0006). Any other cross-strain touch does nothing.
            MatingEnum strain1 = mate1.GetMatingEnum();
            MatingEnum strain2 = mate2.GetMatingEnum();
            if (strain1 != strain2 && !IsHornyMix(strain1, strain2))
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

            // Large cells pair instead of mating, a Horny + Ordinary pair included.
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
                // An Ordinary offspring is never mutated, though a Predator may arrive beside it.
                CreateAndInitializeCell(MatingEnum.Default, newSize, spawnPos);
                TryBringPredator(MatingEnum.Default, newSize, spawnPos, cellVisualConfig.GetBodyRadius(newSize));
            }
            else
            {
                // A Horny parent is involved. The offspring is always Horny, so mating never shifts a strain
                // (only Acid does), and the mating yields a Brood of copies at the parents' stage.
                // In a mixed pair the partner is the Ordinary parent; for Horny + Horny either will do.
                MatingEnum partnerStrain = strain1 == MatingEnum.Horny ? strain2 : strain1;
                HandleMutationAndCreation(MatingEnum.Horny, newSize, spawnPos, deviation1, deviation2);
                SpawnBrood(partnerStrain, stage, spawnPos, deviation1, deviation2);
                // The Predator has to clear the Brood as well as the offspring.
                float clusterRadius = Mathf.Max(cellVisualConfig.GetBodyRadius(newSize), BroodScatter + cellVisualConfig.GetBodyRadius(stage));
                TryBringPredator(MatingEnum.Horny, newSize, spawnPos, clusterRadius);
            }
            mate1.Destroy();
            mate2.Destroy();
        }

        // The extra cells a Horny mating yields besides its offspring: one copy of the partner, sometimes a
        // second, and sometimes a copy of the Horny parent. Rolled once per mating, never per parent. Each
        // "sometimes" grows likelier with every mating that misses it and resets once it happens. Every
        // copy, an Ordinary one included, rolls for a mutation, but none is ever a Predator.
        private void SpawnBrood(MatingEnum partnerStrain, CellSize stage, Vector3 at, DeviationEnum deviation1, DeviationEnum deviation2)
        {
            int partnerCopies = RollGrowingChance(matingConfig.SecondPartnerCopyChance, matingConfig.SecondPartnerCopyChanceGrowth, ref secondPartnerCopyMisses) ? 2 : 1;
            for (int i = 0; i < partnerCopies; i++)
            {
                HandleMutationAndCreation(partnerStrain, stage, at + (Vector3)(UnityEngine.Random.insideUnitCircle * BroodScatter), deviation1, deviation2);
            }

            if (RollGrowingChance(matingConfig.SelfCopyChance, matingConfig.SelfCopyChanceGrowth, ref selfCopyMisses))
            {
                HandleMutationAndCreation(MatingEnum.Horny, stage, at + (Vector3)(UnityEngine.Random.insideUnitCircle * BroodScatter), deviation1, deviation2);
            }
        }

        // A chance that gains `growth` percentage points for every miss in a row, up to 100, and falls back to
        // `startChance` on a hit. Counting misses rather than storing the chance keeps Inspector tuning live.
        private static bool RollGrowingChance(int startChance, int growth, ref int misses)
        {
            int chance = Mathf.Min(100, startChance + misses * growth);
            if (UnityEngine.Random.Range(0, 100) < chance)
            {
                misses = 0;
                return true;
            }

            misses++;
            return false;
        }

        // Predators arrive beside new cells, never in their place (ADR-0008): at most one per mating or
        // Promotion. It takes the strain and stage of the cell it accompanies, and lands a clear gap past
        // the edge of every new cell, so it doesn't kill them the moment both have grown into their colliders.
        // clusterRadius is how far from `at` the new cells reach.
        private void TryBringPredator(MatingEnum strain, CellSize stage, Vector3 at, float clusterRadius)
        {
            int chance = matingProbabilities.GetProbability(DeviationEnum.Red) + mutationChance;
            if (UnityEngine.Random.Range(0, 100) >= chance)
            {
                return;
            }

            float bodyRadius = cellVisualConfig.GetBodyRadius(stage);
            float distance = clusterRadius + bodyRadius + matingConfig.PredatorArrivalGap;
            var walls = new ContactFilter2D { useTriggers = false };
            walls.SetLayerMask(LayerMask.GetMask(WallLayer));

            // Near a rim some directions would put the Predator past a dish wall, or in the next dish. Try
            // others; if the walls leave no room at all, no Predator arrives.
            for (int attempt = 0; attempt < matingConfig.PredatorArrivalAttempts; attempt++)
            {
                float angle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
                Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                Vector2 candidate = (Vector2)at + direction * distance;
                if (Physics2D.Linecast(at, candidate + direction * bodyRadius, walls, wallHits) == 0)
                {
                    SpawnPredator(strain, stage, candidate);
                    return;
                }
            }
        }

        public void SpawnPredator(MatingEnum strain, CellSize stage, Vector3 at)
        {
            FMODUnity.RuntimeManager.PlayOneShot(MutateSoundID);
            redCellFactory.Create().Initialize(strain, stage, at);
        }

        // The closest a Predator's centre may arrive to a lone new cell of its own stage.
        public float PredatorClearance(CellSize stage) => 2f * cellVisualConfig.GetBodyRadius(stage) + matingConfig.PredatorArrivalGap;

        // A Horny cell mates and pairs with an Ordinary one as well as with its own strain (ADR-0006).
        private static bool IsHornyMix(MatingEnum strain1, MatingEnum strain2)
        {
            return (strain1 == MatingEnum.Horny && HornyStrategy.AcceptsPartner(strain2))
                   || (strain2 == MatingEnum.Horny && HornyStrategy.AcceptsPartner(strain1));
        }

        // Two Large cells of the same strain, or a Large Horny and a Large Ordinary, that touch secrete
        // Acid. This replaces the old "Large cell dies -> drops Acid" hook that used to live in
        // LargeSize.HandleOnDie. Acid should cost cells, so both are destroyed (ADR-0001, amended).
        // Agressive cells never get here: they don't pair.
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

            bool hornyMix = IsHornyMix(cell1.MatingEnum, cell2.MatingEnum);
            if ((cell1.MatingEnum != cell2.MatingEnum && !hornyMix) || cell1.MatingEnum == MatingEnum.Acid)
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
            // In a mixed pair the Horny cell sets the yield, whichever collider reported the contact.
            SecreteAcid(hornyMix ? MatingEnum.Horny : cell1.MatingEnum, spawnPos);
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
            
            Vector3 promotedAt = cellMate.GetTargetPosition();
            HandleMutationAndCreation(acid.GrantsStrain, cellMate.CellSize, promotedAt, cellMate.GetView().Deviation, acid.GetView().Deviation);
            // A Promotion always yields the promoted cell; a Predator may arrive beside it (ADR-0008).
            TryBringPredator(acid.GrantsStrain, cellMate.CellSize, promotedAt, cellVisualConfig.GetBodyRadius(cellMate.CellSize));
            strainDiscovery.Discover(acid.GrantsStrain);

            cellMate.Destroy();
            acid.Destroy();
            return true;
        }

        // An Agressive cell touching a cell it can devour destroys it, and the prey pays out its energy
        // as any death does; a Large Horny prey also leaves a Horny Acid. Its Nourishment goes to the
        // devourer; enough of it grows the devourer a stage in place, carrying over the excess, and a Large
        // devourer Bursts instead. A meal short of either heals the devourer a little.
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
            // A devoured Large Horny cell leaves one Horny Acid where it stood. Not a Promotion, so it
            // Discovers nothing.
            if (prey.CellSize == CellSize.Large && prey.MatingEnum == MatingEnum.Horny)
            {
                SpawnAcid(hornyAcidFactory, prey.GetTargetPosition());
                FMODUnity.RuntimeManager.PlayOneShot(SecretionSoundID);
            }
            prey.Destroy();

            int needed = matingConfig.GetNourishmentToGrow(devourer.CellSize);
            if (nourishment < needed)
            {
                devourer.Nourish(nourishment);
                // Only here: growing restores the full lifespan, and Bursting ends it.
                devourer.Heal(matingConfig.LifeRestoredPerMeal);
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

        // Which Acid a pairing yields is set by the strain that paired, and any pair with a Horny cell counts
        // as Horny. Agressive cells never pair; Horny is the terminal strain, so it has no Acid of its own
        // and yields one of each kind instead.
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

        // Rolls only Yellow and Blue. Red never alters a cell: a Predator arrives beside it instead, through
        // TryBringPredator. Weights at or below 0 switch a deviation off (the asset holds -1 for both today).
        private void HandleMutationAndCreation(MatingEnum resultEnum, CellSize newSize, Vector3 spawnPos, DeviationEnum parent1Deviation, DeviationEnum parent2Deviation)
        {
            int yellowProb = Mathf.Max(0, matingProbabilities.GetProbability(DeviationEnum.Yellow));
            int blueProb = Mathf.Max(0, matingProbabilities.GetProbability(DeviationEnum.Blue));

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
            
            int totalMutationChance = yellowProb + blueProb;
            int mutationProbability = UnityEngine.Random.Range(0, 100);
            Debug.Log(totalMutationChance + "           " + mutationProbability);

            if (mutationProbability < totalMutationChance)
            {
                FMODUnity.RuntimeManager.PlayOneShot(MutateSoundID);

                int selectionRoll = UnityEngine.Random.Range(0, totalMutationChance);
                DeviationEnum deviation = selectionRoll < blueProb ? DeviationEnum.Blue : DeviationEnum.Yellow;
                cellFactory.Create().Initialize(resultEnum, newSize, deviation, spawnPos);
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