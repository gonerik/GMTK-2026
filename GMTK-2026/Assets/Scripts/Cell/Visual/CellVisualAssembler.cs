using System;
using DG.Tweening;
using Interfaces;
using UnityEngine;
using Zenject;

namespace DefaultNamespace
{
    public class CellVisualAssembler : MonoBehaviour
    {
        [Inject] private CellVisualConfig config;
        
        private IVisualyConfigurable cell;
        private SpriteRenderer spriteRenderer;
        
        
        private void Awake()
        {
            cell = GetComponent<IVisualyConfigurable>();
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            cell.OnReinitialized += Reassemble;
            Reassemble(cell);
        }

        private void Reassemble(IVisualyConfigurable cell)
        {
            cell.GetTransform().localScale = Vector3.zero;
            spriteRenderer.sprite = config.GetCellSprite(cell.MatingEnum);
            if (cell.Deviation == DeviationEnum.Red)
            {
                spriteRenderer.color = Color.red;
            }
            cell.GetTransform().DOScale(config.GetCellSizeModifier(cell.CellSize), 0.2f);
        }
    }
}