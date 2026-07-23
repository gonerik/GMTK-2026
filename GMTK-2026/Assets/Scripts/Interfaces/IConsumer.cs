using System;

namespace Interfaces
{
    public interface IConsumer : IEntity
    {
        public void Consume(IEatable eatable);
        public void AddEatRule(Predicate<IEatable> rule);
        public bool CanBeEaten(IEatable eatable);
        public void TryGrow();
        public float DetectionRange { get; }
        public int GrowThreshold { get; set; }
    }
}