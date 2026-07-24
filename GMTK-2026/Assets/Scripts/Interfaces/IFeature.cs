namespace Interfaces
{
    public interface IFeature
    {
        public void Initialize(Cell cell);
        public void Unsubscribe(Cell cell);
    }
}