using CameraControl;
using Cinemachine;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

[RequireComponent(typeof(Button))]
public class CameraSwitchButton : MonoBehaviour
{
    [SerializeField] private PetriDishCameraEnum cameraToSwitch;
    
    private Button button;
    
    [Inject] CameraResolver cameraResolver;
    [Inject] CinemachineBrain brain;
    
    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void Start()
    {
        button.onClick.AddListener(OnButtonClick);
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(OnButtonClick);
    }

    private void OnButtonClick()
    {
        SwitchCamera();
    }

    private void SwitchCamera()
    {
        CinemachineVirtualCamera  camera = cameraResolver.ResolveCamera(cameraToSwitch);
        if (camera == null)
        {
            Debug.LogError($"Camera for {cameraToSwitch} not found.");
            return;
        }
        brain.ActiveVirtualCamera.Priority = 0;
        camera.Priority = 10;
    }
}
