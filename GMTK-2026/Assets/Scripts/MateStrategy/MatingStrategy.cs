using MateStrategy;

namespace DefaultNamespace
{
    public class DefaultMatingStrategy : IMateStrategy
    {
        public void Initialize(CellUnit cell)
        {
            cell.SetVisionRange(0);
        }

        public void Unsubscribe(CellUnit cell)
        {

        }
    }

    public class AgressiveStrategy : IMateStrategy
    {
        public void Initialize(CellUnit cell)
        {
            if (cell.GetView().Deviation == DeviationEnum.Red)
            {
                cell.AddTargetingRule(x => x.Deviation != DeviationEnum.Red && x.CellSize == cell.GetView().CellSize);
                return;
            }
            // Agressive cells never mate or pair (ADR-0005): at every stage they hunt what they can devour.
            cell.AddTargetingRule(x => CanDevour(cell, x));
        }

        public void Unsubscribe(CellUnit cell)
        {
        }

        // Shared by targeting and by MatingService, so a cell only chases what it can actually devour:
        // any cell of its own stage or smaller, except another Agressive cell, a Predator or Acid.
        // Acid's CellSize ranks below Small, so it has to be excluded by name.
        public static bool CanDevour(CellUnit devourer, AIView prey)
        {
            return devourer.MatingEnum == MatingEnum.Agressive
                   && prey.CellSize != CellSize.Acid
                   && prey.CellSize <= devourer.CellSize
                   && prey.MatingEnum != MatingEnum.Agressive
                   && prey.Deviation != DeviationEnum.Red;
        }
    }

    public class HornyStrategy : IMateStrategy
    {
        public void Initialize(CellUnit cell)
        {
            if (cell.GetView().Deviation == DeviationEnum.Red)
            {
                cell.AddTargetingRule(x => x.Deviation != DeviationEnum.Red && x.CellSize == cell.CellSize);
                return;
            }
            if(cell.GetView().CellSize == CellSize.Large) return;
            // Only chase partners this cell can actually mate with: same stage, Horny or Ordinary.
            cell.AddTargetingRule(x => cell.GetView().CellSize == x.CellSize && AcceptsPartner(x.MatingEnum) && x.Deviation != DeviationEnum.Red);
        }

        public void Unsubscribe(CellUnit cell)
        {

        }

        // Horny cells mate with Horny or Ordinary cells (ADR-0006). Shared by targeting and by
        // MatingService's strain gate, so a Horny cell only chases what it can actually mate with.
        public static bool AcceptsPartner(MatingEnum strain)
        {
            return strain == MatingEnum.Horny || strain == MatingEnum.Default;
        }
    }

}