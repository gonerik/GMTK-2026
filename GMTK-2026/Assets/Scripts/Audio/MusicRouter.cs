using Zenject;

namespace Audio
{
    public class EnterMainMenuSignal { }
    public class EnterGameplaySignal { }
    public class EnterCreditsSignal { }

    public sealed class MusicRouter : IInitializable, System.IDisposable
    {
        private readonly SignalBus bus;
        private readonly IMusicService music;

        public MusicRouter(SignalBus bus, IMusicService music)
        {
            this.bus = bus;
            this.music = music;
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
        }
        
    }
}
