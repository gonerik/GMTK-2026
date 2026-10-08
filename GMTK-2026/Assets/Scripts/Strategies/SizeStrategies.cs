using DefaultNamespace;
using Interfaces;
using MateStrategy;
using UnityEngine;
using Zenject;

namespace DefaultNamespace.Strategies
{
    public class SmallSize : ISize
    {
        private const string smallSpawnSound = "event:/Cell appears";
        public void Initialize(CellUnit cell)
        {
            cell.SetSpawnSound(smallSpawnSound);
            // Agressive cells devour and never take Acid (ADR-0005), so they don't seek it.
            if (cell.MatingEnum == MatingEnum.Agressive)
            {
                return;
            }
            // Only seek Acid this cell can actually consume; otherwise it parks on the wrong kind.
            cell.AddTargetingRule(view => view.CellSize == CellSize.Acid && view.AcidConsumes == cell.MatingEnum);
        }

        public void Unsubscribe(CellUnit cell)
        {
        }
    }

    public class MediumSize : ISize
    {
        private const string mediumSpawnSound = "event:/Cell becomes mid";
        public void Initialize(CellUnit cell)
        {
            cell.SetSpawnSound(mediumSpawnSound);
        }

        public void Unsubscribe(CellUnit cell)
        {
        }
    }

    public class LargeSize : ISize
    {
        private const string largeSpawnSound = "event:/Cell becomes big";

        public void Initialize(CellUnit cell)
        {
            cell.SetSpawnSound(largeSpawnSound);

            // Ordinary cells never hunt: DefaultMatingStrategy zeroes their vision range, so they
            // wander and let hunters come to them. Agressive cells never pair; they devour
            // (AgressiveStrategy). Horny cells look for a same-strain partner to pair with. Pairing
            // destroys both cells, so the HasPaired checks only matter within the frame it happens.
            if (cell.MatingEnum == MatingEnum.Horny)
            {
                cell.AddTargetingRule(view => !cell.HasPaired
                                              && view.CellSize == CellSize.Large
                                              && view.MatingEnum == cell.MatingEnum
                                              && view.Deviation == DeviationEnum.Default
                                              && !view.HasPaired);
            }
        }

        public void Unsubscribe(CellUnit cell)
        {
        }
    }
}
