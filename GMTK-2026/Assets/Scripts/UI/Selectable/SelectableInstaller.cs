using Zenject;

namespace Cell.Selectable
{
    public class SelectableInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<SelectableController>().AsSingle();
        }
    }
}