using Audio;
using CoreLoop.Interfaces;
using Cell.Visual;
using DefaultNamespace;
using DefaultNamespace.Zenject;
using Dragging;
using Energy;
using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{
    [SerializeField] private GlobalTimer globalTimerPrefab;
    [SerializeField] private Texture2D dragCursor;

    public override void InstallBindings()
    {
        Container.Bind<DefaultActions>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<DragController>().AsSingle();
        // SelectableController listens to the same press and must run first, so it sees the clicked cell
        // before Pickup disables its collider. This installer precedes SelectableInstaller in the
        // SceneContext, so without an explicit order DragController would subscribe first.
        Container.BindExecutionOrder<DragController>(10);
        if (dragCursor != null)
        {
            Container.BindInstance(dragCursor).WhenInjectedInto<DragController>();
        }
        SignalBusInstaller.Install(Container);
        //Container.Bind<ISceneLoader>().To<SceneLoader>().AsSingle();
        Container.BindInterfacesAndSelfTo<CellVisualAssembler>().AsTransient().NonLazy();
        Container.BindInterfacesAndSelfTo<EnergyService>().AsSingle().NonLazy();
        Container.DeclareSignal<EnergyService.OnEnergyGoalReachedSignal>();
        Container.DeclareSignal<EnergyService.OnEnergyLostSignal>();
        Container.DeclareSignal<GlobalTimer.OnLoseSignal>();
        Container.BindInterfacesAndSelfTo<NavigationSystem>().AsSingle().NonLazy();
        Container.Bind<GlobalTimer>()
            .FromComponentInNewPrefab(globalTimerPrefab)
            .AsSingle()
            .NonLazy();
    }
}
