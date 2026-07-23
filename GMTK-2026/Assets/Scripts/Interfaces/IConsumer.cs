namespace Interfaces
{
    public interface IConsumer : IEntity
    {
        public void Consume(IEatable eatable);
        public void Grow();
        public float DetectionRange { get; }
    }
}