using DefaultNamespace;
using UnityEditor;

namespace Interfaces
{
    public interface IEntity
    {
        public CellSize CellSize { get; }
        public void Destroy();
    }
}