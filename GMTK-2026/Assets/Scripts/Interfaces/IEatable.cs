using DefaultNamespace.GrowStrategy;

namespace Interfaces
{
    public interface IEatable : IEntity
    {
        public IGrowStrategy Eat(IConsumer consumer);
        public void Destroy();
        
        
    }
}