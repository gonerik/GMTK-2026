using DefaultNamespace;
using DG.Tweening;
using Interfaces;
using UnityEngine;
using Zenject;

namespace Cell.Visual
{
    public class CellVisualAssembler
    {
        [Inject] private CellVisualConfig config;

        public SpriteRenderer Reassemble(IVisualyConfigurable cell)
        {
            cell.GetTransform().localScale = Vector3.zero;

            foreach (Transform child in cell.GetTransform())
            {
                UnityEngine.Object.Destroy(child.gameObject);
            }
            
            SpriteRenderer spritePrefab = config.GetCellSprite(cell.MatingEnum, cell.CellSize);
            Object.Destroy(cell.GetTransform().GetComponentInChildren<SpriteRenderer>());
            SpriteRenderer sprite = UnityEngine.Object.Instantiate(spritePrefab, cell.GetTransform());
            sprite.color = config.GetDeviationColor(cell.Deviation);
            cell.GetTransform().DOScale(config.GetCellSizeModifier(cell.CellSize), 0.2f);
            return sprite;
        }
    }
}