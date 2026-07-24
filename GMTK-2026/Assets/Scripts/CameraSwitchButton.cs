using UnityEngine;
using UnityEngine.UI;
using Zenject;

[RequireComponent(typeof(Button))]
public class CameraSwitchButton : MonoBehaviour
{
    [SerializeField] private PetriDishCameraEnum cameraToSwitch;
    private Button button;
    [Inject] private CameraSwitchInstaller cameraSwitchInstaller;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void Start()
    {
        button.onClick.AddListener(OnButtonClick);
    }

    private void OnButtonClick()
    {
        cameraSwitchInstaller.SwitchCamera(cameraToSwitch);
    }
}
