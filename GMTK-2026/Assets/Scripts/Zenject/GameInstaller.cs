using Audio;
using CoreLoop.Interfaces;
using Cell;
using Cell.Visual;
using DefaultNamespace;
using DefaultNamespace.Zenject;
using Dragging;
using Energy;
using UI.Reel;
using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{
    [SerializeField] private GlobalTimer globalTimerPrefab;
    [SerializeField] private Texture2D dragCursor;
    [SerializeField] private EnergyService.Settings energySettings = new EnergyService.Settings();

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
        Container.BindInstance(energySettings);
        Container.BindInterfacesAndSelfTo<EnergyService>().AsSingle().NonLazy();
        Container.DeclareSignal<EnergyService.OnEnergyGoalReachedSignal>();
        Container.DeclareSignal<EnergyService.OnEnergyLostSignal>();
        // Plain Bind, not BindInterfacesAndSelfTo: EnergyService already holds the IMeter binding. Its numbers
        // come from the FoodConfig asset, a ScriptableObjectInstaller on the SceneContext.
        Container.Bind<FoodService>().AsSingle();
        Container.DeclareSignal<GlobalTimer.OnLoseSignal>();
        Container.Bind<StrainDiscovery>().AsSingle();
        Container.DeclareSignal<StrainDiscovery.StrainDiscoveredSignal>();
        Container.DeclareSignal<ReelSelectorController.SeedStrainChangedSignal>();
        Container.DeclareSignal<CameraSwitchButton.DishSelectedSignal>();
        Container.BindInterfacesAndSelfTo<NavigationSystem>().AsSingle().NonLazy();
        Container.Bind<GlobalTimer>()
            .FromComponentInNewPrefab(globalTimerPrefab)
            .AsSingle()
            .NonLazy();
    }
}
