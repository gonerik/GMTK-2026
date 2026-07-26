using DefaultNamespace;
using Interfaces;
using UnityEngine;
using Zenject;

namespace DefaultNamespace.Strategies
{
    public class SmallSize : ISize
    {
        public void Initialize(CellUnit cell)
        {
            cell.AddTargetingRule(view => view.CellSize == CellSize.Acid);
        }

        public void Unsubscribe(CellUnit cell)
        {
        }
    }

    public class MediumSize : ISize
    {
        public void Initialize(CellUnit cell)
        {
        }

        public void Unsubscribe(CellUnit cell)
        {
        }
    }

    public class LargeSize : ISize
    {
        private Acid.Factory acidFactory;

        public LargeSize(Acid.Factory acidFactory)
        {
            this.acidFactory = acidFactory;
        }

        public void Initialize(CellUnit cell)
        {
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
                var acid = acidFactory.Create();
                acid.transform.position = cell.transform.position;
            }
        }
    }
}
