using Zenject;

namespace Audio
{
    /// <summary>
    /// Simple, code-only mapping from Zenject signals to sounds.
    /// Mirrors the style used in TutorialController: explicit generic subscriptions in code.
    /// </summary>
    public sealed class SoundSignalRouterTyped : IInitializable, System.IDisposable
    {
        private readonly SignalBus signalBus;
        private readonly ISoundService sound;

        public SoundSignalRouterTyped(SignalBus signalBus, ISoundService sound)
        {
            this.signalBus = signalBus;
            this.sound = sound;
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
        }
    }
}
