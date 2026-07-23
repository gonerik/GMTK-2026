using System;
using DefaultNamespace.Strategies;
using Interfaces;

namespace DefaultNamespace
{
    public static class CellStrategyFactory
    {
        public static Interfaces.IColor CreateColor(CellColor colorType)
        {
            switch (colorType)
            {
                case CellColor.Green:
                    return new Strategies.GreenColor();
                case CellColor.Yellow:
                    return new Strategies.YellowColor();
                case CellColor.Red:
                    return new Strategies.RedColor();
                default:
                    throw new System.ArgumentOutOfRangeException(nameof(colorType), colorType, null);
            }
        }

        public static Interfaces.IMembrane CreateMembrane(Interfaces.CellMembrane membraneType)
        {
            switch (membraneType)
            {
                case Interfaces.CellMembrane.Default:
                    return new Strategies.DefaultMembrane();
                case Interfaces.CellMembrane.Spike:
                    return new Strategies.SpikeMembrane();
                case Interfaces.CellMembrane.Fluid:
                    return new Strategies.FluidMembrane();
                default:
                    throw new System.ArgumentOutOfRangeException(nameof(membraneType), membraneType, null);
            }
        }
    }
}
