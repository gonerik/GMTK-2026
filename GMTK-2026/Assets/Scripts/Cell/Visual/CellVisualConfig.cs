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
        [Tooltip("How far a fully nourished Large Agressive cell swells, relative to its stage scale, just before it Bursts.")]
        [SerializeField] private float burstSwell = 1.5f;

        public override void InstallBindings()
        {
            Container.BindInstance(this).AsSingle();
        }

        public float GetCellSizeModifier(CellSize cellSize) => cellSizeModifiers[cellSize];

        // A partly nourished cell sits between its own stage's scale and the next one's, in proportion to
        // its growth (0..1). Large has no next stage, so it swells toward burstSwell instead.
        public float GetCellScale(CellSize cellSize, float growth)
        {
            float from = cellSizeModifiers[cellSize];
            float to;
            switch (cellSize)
            {
                case CellSize.Small:
                    to = cellSizeModifiers[CellSize.Medium];
                    break;
                case CellSize.Medium:
                    to = cellSizeModifiers[CellSize.Large];
                    break;
                case CellSize.Large:
                    to = from * burstSwell;
                    break;
                default:
                    return from;
            }
            return Mathf.Lerp(from, to, growth);
        }
        public SpriteRenderer GetCellSprite(MatingEnum matingEnum, CellSize cellSize) => matingModifiers[matingEnum][cellSize];
        public Color GetDeviationColor(DeviationEnum deviationEnum) => deviationColors[deviationEnum];
    }
}