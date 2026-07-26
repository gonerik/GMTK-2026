using DefaultNamespace;
using Interfaces;
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
        private Acid.Factory acidFactory;

        public LargeSize(Acid.Factory acidFactory)
        {
            this.acidFactory = acidFactory;
        }

        public void Initialize(CellUnit cell)
        {
            cell.SetSpawnSound(largeSpawnSound);
            cell.OnDie += HandleOnDie;
        }

        public void Unsubscribe(CellUnit cell)
        {
            cell.OnDie -= HandleOnDie;
        }

        private void HandleOnDie(CellUnit cell)
        {
            if (acidFactory != null)
            {
                int count = (int)cell.MatingEnum;
                float randomOffset = Random.Range(0f, 360f);
                for (int i = 0; i < count; i++)
                {
                    var acid = acidFactory.Create();
                    float angle = randomOffset + i * (360f / count);
                    Vector3 offset = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0);
                    acid.transform.position = cell.transform.position + offset;
                }
            }
        }
    }
}
