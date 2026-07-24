namespace Interfaces
{
    public interface IMembrane
    {
        public void Initialize(Cell cell);
        public void Unsubscribe(Cell cell);
    }
}