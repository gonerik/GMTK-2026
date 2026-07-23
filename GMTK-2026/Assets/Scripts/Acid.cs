using System;
using System.Collections.Generic;
using DefaultNamespace.Zenject;
using Interfaces;
using UnityEngine;
using Zenject;

namespace DefaultNamespace
{
    public class Acid : MonoBehaviour, IEatable
    {
        public CellSize CellSize => CellSize.Acid;
        
        private List<Predicate<(IConsumer, IEatable)>> eatRules = new List<Predicate<(IConsumer, IEatable)>>();

        public List<(IConsumer, IEatable)> EatRules { get; }
        
        [Inject] private NavigationSystem navigationSystem;

        private void Start()
        {
            AddEatRule(new Predicate<(IConsumer, IEatable)>((x) => x.Item1.CellSize - 1 == x.Item2.CellSize));
            navigationSystem.RegisterEatable(this);
        }

        private void OnDestroy()
        {
            navigationSystem.UnregisterEatable(this);
        }
		
        public virtual void Eat(IConsumer consumer)
        {
            Destroy(gameObject);
        }

        public void AddEatRule(Predicate<(IConsumer, IEatable)> rule)
        {
            eatRules.Add(rule);
        }

        public bool CanBeEaten(IConsumer consumer)
        {
            bool canBeEaten = true;
            foreach (var rule in eatRules)
            {
                canBeEaten = rule.Invoke((consumer, this)) && canBeEaten;
            }
            return canBeEaten;
        }
    }
}