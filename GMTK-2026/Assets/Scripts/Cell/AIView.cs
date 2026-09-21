using MateStrategy;

namespace DefaultNamespace
{
    public class AIView
    {
        public CellSize CellSize { get; set; }
        public MatingEnum MatingEnum { get; set; }
        public DeviationEnum Deviation { get; set; }
        public bool HasPaired { get; set; }
        // Only meaningful when CellSize is Acid: the strain that Acid can promote.
        public MatingEnum AcidConsumes { get; set; }
    }
}