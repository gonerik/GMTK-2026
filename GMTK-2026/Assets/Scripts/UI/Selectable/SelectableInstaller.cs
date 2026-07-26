using Zenject;

namespace Cell.Selectable
{
    public class SelectableInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<SelectableController>().AsSingle();
        }
    }
}