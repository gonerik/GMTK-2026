using Cinemachine;
using Zenject;

namespace CameraControl
{
    public class CameraResolver
    {
        [Inject(Id = "VirtualCamera1")] private CinemachineVirtualCamera virtualCamera1;
        [Inject(Id = "VirtualCamera2")] private CinemachineVirtualCamera virtualCamera2;    
        [Inject(Id = "VirtualCamera3")] private CinemachineVirtualCamera virtualCamera3;

        public CinemachineVirtualCamera ResolveCamera(PetriDishCameraEnum cameraEnum)
        {
            switch (cameraEnum)
            {
                case PetriDishCameraEnum.Dish1:
                    return virtualCamera1;
                case PetriDishCameraEnum.Dish2:
                    return virtualCamera2;
                case PetriDishCameraEnum.Dish3:
                    return virtualCamera3;
                default:
                    throw new System.ArgumentOutOfRangeException(nameof(cameraEnum), cameraEnum, null);
            }
        }
    }
}