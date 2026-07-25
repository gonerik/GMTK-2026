using AYellowpaper.SerializedCollections;
using DefaultNamespace;
using MateStrategy;
using UnityEngine;
using Zenject;

namespace Cell
{
    [ CreateAssetMenu( fileName = "CellLifetimeConfig", menuName = "DefaultNamespace/CellLifetimeConfig", order = 0 )]
    public class CellLifetimeConfig : ScriptableObjectInstaller
    {
        [SerializeField] private SerializedDictionary<MatingEnum, int> MatingLifetimeAddition;
        [SerializeField] private SerializedDictionary<CellSize, int> SizeLifetimeAddition;
        [SerializeField] private SerializedDictionary<DeviationEnum, int> DeviationLifetimeAddition;
        
        [SerializeField] private SerializedDictionary<MatingEnum, int> MatingEnergyGainAddition;
        [SerializeField] private SerializedDictionary<CellSize, int> SizeEnergyGainAddition;
        [SerializeField] private SerializedDictionary<DeviationEnum, int> DeviationEnergyGainAddition;
        
        public override void InstallBindings()
        {
            Container.BindInstance(this).AsSingle();
        }
        
        public int CalculateLifetime(MatingEnum matingEnum, CellSize cellSize, DeviationEnum deviationEnum)
        {
            int lifetime = MatingLifetimeAddition[matingEnum];
            lifetime += SizeLifetimeAddition[cellSize];
            lifetime += DeviationLifetimeAddition[deviationEnum];
            return lifetime;
        }
        
        public int CalculateEnergyGain(MatingEnum matingEnum, CellSize cellSize, DeviationEnum deviationEnum)
        {
            int energyGain = MatingEnergyGainAddition[matingEnum];
            energyGain += SizeEnergyGainAddition[cellSize];
            energyGain += DeviationEnergyGainAddition[deviationEnum];
            return energyGain;
        }
    }
}