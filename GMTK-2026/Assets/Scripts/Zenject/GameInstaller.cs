using CoreLoop.Interfaces;
using Cell.Visual;
using DefaultNamespace;
using DefaultNamespace.Zenject;
using Energy;
using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<DefaultActions>().AsSingle().NonLazy();
        SignalBusInstaller.Install(Container);
        //Container.Bind<ISceneLoader>().To<SceneLoader>().AsSingle();
        Container.BindInterfacesAndSelfTo<CellVisualAssembler>().AsTransient().NonLazy();
        Container.BindInterfacesAndSelfTo<EnergyService>().AsSingle().NonLazy();
        Container.DeclareSignal<EnergyService.OnEnergyGoalReachedSignal>();
        Container.BindInterfacesAndSelfTo<NavigationSystem>().AsSingle().NonLazy();
    }
}
