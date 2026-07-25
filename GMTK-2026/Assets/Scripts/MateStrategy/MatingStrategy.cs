using MateStrategy;

namespace DefaultNamespace
{
    public class DefaultMatingStrategy : IMateStrategy
    {
        public void Initialize(CellUnit cell)
        {
            cell.AddTargetingRule(x => false);
        }

        public void Unsubscribe(CellUnit cell)
        {

        }
    }

    public class AgressiveStrategy : IMateStrategy
    {
        public void Initialize(CellUnit cell)
        {
            cell.AddTargetingRule(x => cell.GetView().CellSize == x.CellSize && cell.GetView().Deviation == x.Deviation);
            cell.AddTargetingRule(x => cell.GetView().CellSize != CellSize.Large && x.CellSize != CellSize.Large);
        }

        public void Unsubscribe(CellUnit cell)
        {
        }
    }

    public class HornyStrategy : IMateStrategy
    {
        public void Initialize(CellUnit cell)
        {
            cell.AddTargetingRule(x => cell.GetView().CellSize == x.CellSize && cell.GetView().Deviation == x.Deviation);
            cell.AddTargetingRule(x => cell.GetView().CellSize != CellSize.Large && x.CellSize != CellSize.Large);
        }

        public void Unsubscribe(CellUnit cell)
        {
            
        }
    }

}