using Cell.Visual;
using DefaultNamespace;
using DefaultNamespace.Zenject;
using Energy;
using Zenject;

public class GameInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        SignalBusInstaller.Install(Container);
        Container.BindInterfacesAndSelfTo<CellVisualAssembler>().AsTransient().NonLazy();
        Container.BindInterfacesAndSelfTo<EnergyService>().AsSingle().NonLazy();
        Container.DeclareSignal<EnergyService.OnEnergyGoalReachedSignal>();
        Container.BindInterfacesAndSelfTo<NavigationSystem>().AsSingle().NonLazy();
    }
}
