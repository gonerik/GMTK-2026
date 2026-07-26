using Audio;
using Audio.Interfaces;
using UnityEngine;
using Zenject;

namespace Installers
{
    public class SoundInstaller : MonoInstaller
    {

        public override void InstallBindings()
        {
            Container.Bind<MusicService>().AsSingle().NonLazy();
            Container.Bind<IAudioManager>().To<AudioManager>().AsSingle();
        }
    }
}
