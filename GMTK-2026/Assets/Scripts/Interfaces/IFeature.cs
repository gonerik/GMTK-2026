namespace Interfaces
{
    public interface IFeature
    {
        public void Initialize(CellUnit cell);
        public void Unsubscribe(CellUnit cell);
    }
}