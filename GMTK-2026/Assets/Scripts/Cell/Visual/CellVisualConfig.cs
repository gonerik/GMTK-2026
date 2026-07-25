using AYellowpaper.SerializedCollections;
using MateStrategy;
using UnityEngine;
using Zenject;

namespace DefaultNamespace
{
    [CreateAssetMenu(fileName = "CellVisualConfig", menuName = "DefaultNamespace/CellVisualConfig", order = 0)]
    public class CellVisualConfig : ScriptableObjectInstaller
    {
        [SerializeField] private SerializedDictionary<CellSize, float> cellSizeModifiers;
        [SerializeField] private SerializedDictionary<MatingEnum, SerializedDictionary<CellSize, SpriteRenderer>> matingModifiers;
        
        public override void InstallBindings()
        {
            Container.BindInstance(this).AsSingle();
        }
        
        public float GetCellSizeModifier(CellSize cellSize) => cellSizeModifiers[cellSize];
        public SpriteRenderer GetCellSprite(MatingEnum matingEnum, CellSize cellSize) => matingModifiers[matingEnum][cellSize];
    }
}