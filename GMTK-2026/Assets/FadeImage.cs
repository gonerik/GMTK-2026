using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class FadeImage : MonoBehaviour
{
    private Image image;

    private void Start()
    {
        image = GetComponent<Image>();
    }

    public void Fade()
    {
        image.DOFade(0, 0.7f);
    }
    
    public void FadeReverse()
    {
        image.DOFade(1, 1);
    }
}
