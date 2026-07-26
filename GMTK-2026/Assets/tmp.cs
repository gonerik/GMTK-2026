using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class tmp : MonoBehaviour
{
    private PlayableDirector director;
    
    [SerializeField] private bool isPlaying;
    
    private void Start()
    {
        director = GetComponent<PlayableDirector>();
    }

    private void Update()
    {
        if(isPlaying) director.Play();
    }
}
