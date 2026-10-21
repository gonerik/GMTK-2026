using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Drives the adaptive music and the progress sprite. It no longer ends the run: only Energy does.
public class GlobalTimer : MonoBehaviour
{
    [Header("Timer")]
    public float totalTime = 100f;   // total duration of your timer
    private float elapsedTime = 0f;

    private bool _goTo2Triggered;
    private bool _goTo3Triggered;

    public float ProgressTime => elapsedTime / totalTime;
    public float ElapsedTime => elapsedTime;
    
    void Update()
    {
        elapsedTime += Time.deltaTime;
        elapsedTime = Mathf.Clamp(elapsedTime, 0f, totalTime);

        float progress = ProgressTime;
        if (!_goTo2Triggered && progress >= 0.33f)
        {
            _goTo2Triggered = true;
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName("Go To 2", 1);
        }
        
        if (!_goTo3Triggered && progress >= 0.66f)
        {
            _goTo3Triggered = true;
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName("Go To 3", 1);
        }
    }
}
