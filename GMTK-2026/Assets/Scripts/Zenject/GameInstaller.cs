using DefaultNamespace.Zenject;
using Zenject;

public class GameInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.BindInterfacesAndSelfTo<NavigationSystem>().AsSingle().NonLazy();
    }
}
