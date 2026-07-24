using DefaultNamespace.Zenject;
using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.BindInterfacesAndSelfTo<NavigationSystem>().AsSingle().NonLazy();
    }
}
