namespace Energy
{
    // Where a dish sends a dying cell's positive Payout. A loss always costs Energy, whatever the dish.
    // Serialized on every dish as its ordinal, so keep the order. Energy comes first so a cell that dies
    // before entering any dish pays Energy.
    public enum DishPayout
    {
        Energy,
        Food,
        Nothing
    }
}
