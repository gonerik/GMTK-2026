using System;

namespace Energy
{
    // A board-wide amount shown on a bar.
    public interface IMeter
    {
        int Current { get; }
        int Max { get; }
        event Action<int> OnChanged;
    }

    // Which meter a bar shows. Serialized on the bars as its ordinal, so keep the order.
    public enum MeterKind
    {
        Energy,
        Food
    }
}
