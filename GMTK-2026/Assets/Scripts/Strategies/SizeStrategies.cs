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
            cell.AddTargetingRule(view => view.CellSize == CellSize.Acid);
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
            // wander and let hunters come to them. Agressive and Horny cells look for a same-strain
            // partner to pair with. The closure reads cell.HasPaired live, so a cell stops hunting
            // the moment it secretes, with no re-initialisation.
            if (cell.MatingEnum == MatingEnum.Agressive || cell.MatingEnum == MatingEnum.Horny)
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
