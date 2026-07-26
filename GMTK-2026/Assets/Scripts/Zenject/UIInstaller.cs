using UI;
using UnityEngine;
using Zenject;

namespace Installers
{
    public class UIInstaller : MonoInstaller
    {
        [SerializeField] private SettingMenu settingMenu;
        //[SerializeField] private PauseMenu pauseMenu;
        public override void InstallBindings()
        {
            Container.Bind<SettingMenu>().FromComponentInNewPrefab(settingMenu).AsSingle().NonLazy();
            //Container.Bind<PauseMenu>().FromComponentInNewPrefab(pauseMenu).AsSingle().NonLazy();
        }
    }
}