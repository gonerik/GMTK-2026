using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class FadeGroupFade : MonoBehaviour
{
    private CanvasGroup fadeGroup;
    
    private void Start()
    {
        fadeGroup = GetComponent<CanvasGroup>();
    }
    
    public void Fadeout()
    {
        fadeGroup.DOFade(0f, 1f).SetUpdate(true);
    }
}
