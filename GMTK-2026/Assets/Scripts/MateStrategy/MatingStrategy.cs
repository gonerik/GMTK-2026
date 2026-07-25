using MateStrategy;

namespace DefaultNamespace
{
    public class DefaultMatingStrategy : IMateStrategy
    {
        public void Initialize(Cell cell)
        {
            cell.AddTargetingRule(x => false);
        }

        public void Unsubscribe(Cell cell)
        {

        }
    }

    public class AgressiveStrategy : IMateStrategy
    {
        public void Initialize(Cell cell)
        {
            cell.AddTargetingRule(x => cell.GetView().CellSize == x.CellSize);
        }

        public void Unsubscribe(Cell cell)
        {
        }
    }

    public class HornyStrategy : IMateStrategy
    {
        public void Initialize(Cell cell)
        {
            cell.AddTargetingRule(x => cell.GetView().CellSize == x.CellSize);
        }

        public void Unsubscribe(Cell cell)
        {
            
        }
    }

}