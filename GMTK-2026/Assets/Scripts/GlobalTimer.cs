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

    private const string winSound = "event:/Win";
    
    public float ProgressTime => elapsedTime / totalTime;
    public float ElapsedTime => elapsedTime;
    
    void Update()
    {
        elapsedTime += Time.deltaTime;
        elapsedTime = Mathf.Clamp(elapsedTime, 0f, totalTime);
        if(elapsedTime >= totalTime)
        {
            FMODUnity.RuntimeManager.PlayOneShot(winSound);
            signalBus.Fire<OnLoseSignal>();
        }
    }
}
