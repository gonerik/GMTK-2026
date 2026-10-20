using DefaultNamespace;
using Energy;
using MateStrategy;

namespace Interfaces
{
    public struct SelectionInfo
    {
        public CellSize CellSize;
        public DeviationEnum Deviation;
        public MatingEnum MatingEnum;
        public int EnergyAmount;
        public int FoodAmount;
        // Which Payout the cell's death will pay. Defaults to Energy, which is what Predators always pay.
        public DishPayout Payout;
        public float speed;
        public float Age;
        public int MaxAge;
        public bool HasPaired;
    }
    public interface ISelectable
    {
        public SelectionInfo GetSelectionInfo();
    }
}