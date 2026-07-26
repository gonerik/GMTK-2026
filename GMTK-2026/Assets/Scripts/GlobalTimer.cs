using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalTimer : MonoBehaviour
{
    [Header("Timer")]
    public float totalTime = 100f;   // total duration of your timer
    private float elapsedTime = 0f;
    
    public float ProgressTime => elapsedTime / totalTime;
    public float ElapsedTime => elapsedTime;
    
    void Update()
    {
        elapsedTime += Time.deltaTime;
        elapsedTime = Mathf.Clamp(elapsedTime, 0f, totalTime);
    }
}
