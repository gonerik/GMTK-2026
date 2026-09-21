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
            // Only chase partners this cell can actually mate with: same stage and same strain.
            if(cell.GetView().CellSize != CellSize.Large)
            {
                cell.AddTargetingRule(x => cell.GetView().CellSize == x.CellSize && x.MatingEnum == cell.MatingEnum && x.Deviation != DeviationEnum.Red);
            }
        }

        public void Unsubscribe(CellUnit cell)
        {
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
            // Only chase partners this cell can actually mate with: same stage and same strain.
            cell.AddTargetingRule(x => cell.GetView().CellSize == x.CellSize && x.MatingEnum == cell.MatingEnum && x.Deviation != DeviationEnum.Red);
        }

        public void Unsubscribe(CellUnit cell)
        {
            
        }
    }

}