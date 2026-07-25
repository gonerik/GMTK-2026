using CoreLoop.Interfaces;
using DefaultNamespace.Zenject;
using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<DefaultActions>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<NavigationSystem>().AsSingle().NonLazy();
    }
}
