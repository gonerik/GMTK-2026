using System;
using System.Collections.Generic;
using DefaultNamespace;

namespace Interfaces
{
    public interface IEatable : IEntity
    {
        public void Eat(IConsumer consumer);
        public void AddEatRule(Predicate<(IConsumer, IEatable)> rule);
        
        public bool CanBeEaten(IConsumer consumer);
        
    }
}