using AYellowpaper.SerializedCollections;
using DefaultNamespace;
using UnityEngine;
using Zenject;

namespace MateStrategy
{
    [CreateAssetMenu(fileName = "MatingProbabilities", menuName = "Installers/MatingProbabilities", order = 0)]
    public class MatingProbabilities : ScriptableObjectInstaller
    {
        [SerializedDictionary] public SerializedDictionary<DeviationEnum, int> Probabilities;
        public override void InstallBindings()
        {
            Container.BindInstance(this).AsSingle();
        }
        
        public int GetProbability(DeviationEnum deviationEnum) => Probabilities[deviationEnum];
    }
}