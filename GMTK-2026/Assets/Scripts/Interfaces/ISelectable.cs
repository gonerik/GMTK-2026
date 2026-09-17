using DefaultNamespace;
using MateStrategy;

namespace Interfaces
{
    public struct SelectionInfo
    {
        public CellSize CellSize;
        public DeviationEnum Deviation;
        public MatingEnum MatingEnum;
        public int EnergyAmount;
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