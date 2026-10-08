using System.Collections.Generic;
using MateStrategy;
using Zenject;

namespace Cell
{
    /// <summary>
    /// Tracks which strains the player has Discovered this run. Ordinary starts Discovered; every other
    /// strain is Discovered by its first Promotion, even one whose mutation roll produced a Predator.
    /// Bound in the scene container, so a scene reload starts a fresh run.
    /// </summary>
    public class StrainDiscovery
    {
        public readonly struct StrainDiscoveredSignal
        {
            public readonly MatingEnum Strain;

            public StrainDiscoveredSignal(MatingEnum strain)
            {
                Strain = strain;
            }
        }

        private const string DiscoverySoundID = "event:/First Find";

        private readonly SignalBus _signalBus;
        private readonly HashSet<MatingEnum> _discovered = new HashSet<MatingEnum> { MatingEnum.Default };

        [Inject]
        public StrainDiscovery(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        public bool IsDiscovered(MatingEnum strain) => _discovered.Contains(strain);

        public void Discover(MatingEnum strain)
        {
            if (!_discovered.Add(strain)) return;

            FMODUnity.RuntimeManager.PlayOneShot(DiscoverySoundID);
            _signalBus.Fire(new StrainDiscoveredSignal(strain));
        }
    }
}
