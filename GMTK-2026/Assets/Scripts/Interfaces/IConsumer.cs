namespace Interfaces
{
    public interface IConsumer : IEntity
    {
        public void Consume(IEatable eatable);
    }
}