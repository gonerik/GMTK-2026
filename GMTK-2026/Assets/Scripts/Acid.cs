using System;
using System.Collections.Generic;
using Interfaces;
using Unity.Burst;
using UnityEngine;

namespace DefaultNamespace
{
    public class Acid : MonoBehaviour, IEatable
    {
        public Size Size => Size.Acid;
        
        private List<Predicate<(IConsumer, IEatable)>> eatRules = new List<Predicate<(IConsumer, IEatable)>>();

        public List<(IConsumer, IEatable)> EatRules { get; }
        
        void Start()
        {
            AddEatRule(new Predicate<(IConsumer, IEatable)>((x) => x.Item1.Size - 1 == x.Item2.Size));
        }
        
        public virtual void Eat(IConsumer consumer)
        {
            gameObject.SetActive(false);
        }

        public void AddEatRule(Predicate<(IConsumer, IEatable)> rule)
        {
            eatRules.Add(rule);
        }

        [BurstCompile]
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