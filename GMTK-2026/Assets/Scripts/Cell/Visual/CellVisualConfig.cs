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
        [SerializeField] private SerializedDictionary<DeviationEnum, Color> deviationColors;
        
        public override void InstallBindings()
        {
            Container.BindInstance(this).AsSingle();
        }
        
        public float GetCellSizeModifier(CellSize cellSize) => cellSizeModifiers[cellSize];
        public SpriteRenderer GetCellSprite(MatingEnum matingEnum, CellSize cellSize) => matingModifiers[matingEnum][cellSize];
        public Color GetDeviationColor(DeviationEnum deviationEnum) => deviationColors[deviationEnum];
    }
}