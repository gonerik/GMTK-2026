using Interfaces;

namespace DefaultNamespace.GrowStrategy
{
    public interface IGrowStrategy
    {
        public void Invoke(IConsumer target);
    }
}