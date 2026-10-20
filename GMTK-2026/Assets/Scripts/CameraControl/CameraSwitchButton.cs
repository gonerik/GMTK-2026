using CameraControl;
using Cinemachine;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

[RequireComponent(typeof(Button))]
public class CameraSwitchButton : MonoBehaviour
{
    // The player selected a dish. Fired after its camera takes over.
    public struct DishSelectedSignal
    {
        public PetriDishCameraEnum Dish;
    }

    [SerializeField] private PetriDishCameraEnum cameraToSwitch;

    private Button button;

    [Inject] CameraResolver cameraResolver;
    [Inject] CinemachineBrain brain;
    [Inject] SignalBus signalBus;
    
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
        signalBus.Fire(new DishSelectedSignal { Dish = cameraToSwitch });
    }
}
