using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class GlobalTimer : MonoBehaviour
{
    public struct OnLoseSignal
    {
        
    }
    [Inject] public SignalBus signalBus; 
    [Header("Timer")]
    public float totalTime = 100f;   // total duration of your timer
    private float elapsedTime = 0f;

    private bool _goTo2Triggered;
    private bool _goTo3Triggered;

    private const string winSound = "event:/Win";
    
    public float ProgressTime => elapsedTime / totalTime;
    public float ElapsedTime => elapsedTime;
    
    void Update()
    {
        elapsedTime += Time.deltaTime;
        elapsedTime = Mathf.Clamp(elapsedTime, 0f, totalTime);

        float progress = ProgressTime;
        if (!_goTo2Triggered && progress >= 0.5f)
        {
            _goTo2Triggered = true;
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName("Go To 2", 1);
        }
        
        if (!_goTo3Triggered && progress >= 0.75f)
        {
            _goTo3Triggered = true;
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName("Go To 3", 1);
        }

        if(elapsedTime >= totalTime)
        {
            FMODUnity.RuntimeManager.PlayOneShot(winSound);
            signalBus.Fire<OnLoseSignal>();
        }
    }
}
