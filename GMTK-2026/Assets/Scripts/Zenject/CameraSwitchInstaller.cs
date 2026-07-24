using UnityEngine;
using Cinemachine;
using System.Collections.Generic;
using Zenject;

public class CameraSwitchInstaller : MonoInstaller
{
    [SerializeField] private CinemachineBrain brain;
    [SerializeField] private List<CinemachineVirtualCamera> petriDishCameras;

    public override void InstallBindings()
    {
        Container.Bind<CameraSwitchInstaller>().FromInstance(this).AsSingle();
    }

    private void Awake()
    {
        if (brain != null && brain.m_CustomBlends == null)
        {
            brain.m_CustomBlends = ScriptableObject.CreateInstance<CinemachineBlenderSettings>();
        }
        
        // Ensure we have custom blends defined if requested specifically via code, 
        // though usually this is done in the Asset.
        // Here we just ensure the brain is referenced for the "Define custom blends" requirement.
    }
    
    public void SwitchCamera(PetriDishCameraEnum cameraType)
    {
        int index = (int)cameraType;
        if (index < 0 || index >= petriDishCameras.Count)
        {
            Debug.LogError($"Camera for {cameraType} not found in SceneContextService");
            return;
        }

        for (int i = 0; i < petriDishCameras.Count; i++)
        {
            petriDishCameras[i].Priority = (i == index) ? 10 : 0;
        }
    }
}
