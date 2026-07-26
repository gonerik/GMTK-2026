using Audio;
using Audio.Interfaces;
using UnityEngine;
using Zenject;

namespace Installers
{
    public class SoundInstaller : MonoInstaller
    {
        [SerializeField] private SoundCatalog catalog;
        [SerializeField] private MusicCatalog musicCatalog;

        public override void InstallBindings()
        {
            Container.Bind<ISoundCatalog>().FromInstance(catalog).AsSingle();

            Container.Bind<ISoundService>().To<SoundService>().AsSingle();
            Container.Bind<IAudioManager>().To<AudioManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<SoundSignalRouterTyped>().AsSingle().NonLazy();

            Container.Bind<MusicCatalog>().FromInstance(musicCatalog).AsSingle();
            Container.BindInterfacesAndSelfTo<MusicService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<MusicRouter>().AsSingle().NonLazy();
        }
    }
}
