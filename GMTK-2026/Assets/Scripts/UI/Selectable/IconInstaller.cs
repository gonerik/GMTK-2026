using AYellowpaper.SerializedCollections;
using DefaultNamespace;
using MateStrategy;
using UnityEngine;
using Zenject;

namespace Cell.Selectable
{
    [ CreateAssetMenu( fileName = "IconInstaller", menuName = "DefaultNamespace/IconInstaller", order = 0 )]
    public class IconInstaller : ScriptableObjectInstaller
    {
        [SerializeField] private SerializedDictionary<MatingEnum, Sprite> MatingIcons;
        [SerializeField] private SerializedDictionary<DeviationEnum, Sprite> DeviationIcons;
        [SerializeField] private SerializedDictionary<CellSize, Sprite> SizeIcons;
        public override void InstallBindings()
        {
            Container.Bind<IconInstaller>().FromInstance(this).AsSingle();
        }
        
        public Sprite GetMatingIcon(MatingEnum matingEnum) => MatingIcons[matingEnum];
        public Sprite GetDeviationIcon(DeviationEnum deviationEnum) => DeviationIcons[deviationEnum];
        public Sprite GetSizeIcon(CellSize cellSize) => SizeIcons[cellSize];
    }
}