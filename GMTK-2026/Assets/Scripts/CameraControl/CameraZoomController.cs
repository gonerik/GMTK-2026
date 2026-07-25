using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.UIElements;
using Zenject;

public class CameraZoomController : ITickable, IInitializable
{
    [Header("Zoom Settings")]
    [SerializeField] private float minOrthoSize = 2f;
    [SerializeField] private float maxOrthoSize = 10f;
    [SerializeField] private float zoomSensitivity = 1f;
    [SerializeField] private float smoothTime = 0.1f;

    [Inject] private CinemachineBrain brain;
    
    private float targetOrthoSize;
    private float currentOrthoVelocity;
    private Vector3 targetPosition;
    private Vector3 currentPositionVelocity;
    
    private Vector3 initialPosition;
    private Camera mainCamera;
    private CinemachineVirtualCamera lastActiveCamera;
    private Dictionary<CinemachineVirtualCamera, Vector3> initialPositions;

    public void Initialize()
    {
        mainCamera = Camera.main;
        if (brain.ActiveVirtualCamera != null)
        {
            lastActiveCamera = brain.ActiveVirtualCamera as CinemachineVirtualCamera;
            if (lastActiveCamera != null)
            {
                targetOrthoSize = lastActiveCamera.m_Lens.OrthographicSize;
                initialPosition = new Vector3(
                    lastActiveCamera.transform.position.x - lastActiveCamera.transform.localPosition.x,
                    lastActiveCamera.transform.position.y - lastActiveCamera.transform.localPosition.y,
                    lastActiveCamera.transform.localPosition.z);
                targetPosition = initialPosition;
            }
        }
    }

    public void Tick()
    {
        var activeCamera = brain.ActiveVirtualCamera as CinemachineVirtualCamera;
        if (activeCamera == null) return;

        if (activeCamera != lastActiveCamera)
        {
            lastActiveCamera = activeCamera;
            targetOrthoSize = activeCamera.m_Lens.OrthographicSize;
            initialPosition = new Vector3(
                lastActiveCamera.transform.position.x - lastActiveCamera.transform.localPosition.x,
                lastActiveCamera.transform.position.y - lastActiveCamera.transform.localPosition.y,
                lastActiveCamera.transform.localPosition.z);
            targetPosition = initialPosition;
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.001f)
        {
            Zoom(scroll, activeCamera);
        }

        // Smoothly update ortho size
        activeCamera.m_Lens.OrthographicSize = Mathf.SmoothDamp(
            activeCamera.m_Lens.OrthographicSize, 
            targetOrthoSize, 
            ref currentOrthoVelocity, 
            smoothTime
        );

        // Smoothly update position
        activeCamera.transform.position = Vector3.SmoothDamp(
            activeCamera.transform.position,
            targetPosition,
            ref currentPositionVelocity,
            smoothTime
        );
    }

    private void Zoom(float scroll, CinemachineVirtualCamera activeCamera)
    {
        float previousOrthoSize = targetOrthoSize;
        targetOrthoSize = Mathf.Clamp(targetOrthoSize - scroll * zoomSensitivity * targetOrthoSize, minOrthoSize, maxOrthoSize);

        if (Mathf.Approximately(previousOrthoSize, targetOrthoSize)) return;

        if (scroll > 0) // Zooming in
        {
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = activeCamera.transform.position.z;
            
            // Calculate where the camera should move to keep the mouse world position under the cursor
            // NewPos = MousePos - (MousePos - OldPos) * (NewSize / OldSize)
            float multiplier = targetOrthoSize / previousOrthoSize;
            Vector3 offset = mouseWorldPos - targetPosition;
            targetPosition = mouseWorldPos - offset * multiplier;
        }
        else // Zooming out
        {
            // When zooming out, pull back towards local 0,0,0 (parent's position or initial position if no parent)
            float zoomPercent = (targetOrthoSize - minOrthoSize) / (maxOrthoSize - minOrthoSize);
            Vector3 localZeroWorldPos = activeCamera.transform.parent.position;
            targetPosition = Vector3.Lerp(targetPosition, initialPosition, zoomPercent);
        }
    }
}
