using System;
using DefaultNamespace.Strategies;
using Interfaces;
using MateStrategy;

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
        
        public static IMateStrategy CreateMateStrategy(MatingEnum matingEnum)
        {
            switch (matingEnum)
            {
                case MatingEnum.Acid: return new DefaultMatingStrategy();
                case MatingEnum.Default: return new DefaultMatingStrategy();
                case MatingEnum.Agressive: return new AgressiveStrategy();
                case MatingEnum.Horny: return new HornyStrategy();
                default:
                    throw new System.ArgumentOutOfRangeException(nameof(matingEnum), matingEnum, null);
            }
        }

        public static ISize CreateSize(CellSize sizeType)
        {
            switch (sizeType)
            {
                case CellSize.Small:
                    return new Strategies.SmallSize();
                case CellSize.Medium:
                    return new Strategies.MediumSize();
                case CellSize.Large:
                    return new Strategies.LargeSize();
                case CellSize.Acid:
                    return new Strategies.MediumSize(); // Acid doesn't use CellUnit usually, but to be safe
                default:
                    throw new ArgumentOutOfRangeException(nameof(sizeType), sizeType, null);
            }
        }
    }
}
