using UnityEngine;
using Cinemachine;
using System.Collections.Generic;
using CameraControl;
using Zenject;

public class CameraSwitchInstaller : MonoInstaller
{
    [SerializeField] private CinemachineBrain brain;

    public override void InstallBindings()
    {
        Container.Bind<CameraResolver>().AsSingle();
        Container.Bind<CinemachineBrain>().FromInstance(brain).AsSingle();
        Container.BindInterfacesAndSelfTo<CameraZoomController>().AsSingle().NonLazy();
    }
    
}
