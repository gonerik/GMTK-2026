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

        public SpriteRenderer Reassemble(IVisualyConfigurable cell, float growth = 0f)
        {
            cell.GetTransform().DOKill();
            cell.GetTransform().localScale = Vector3.zero;

            foreach (Transform child in cell.GetTransform())
            {
                UnityEngine.Object.Destroy(child.gameObject);
            }
            
            SpriteRenderer spritePrefab = config.GetCellSprite(cell.MatingEnum, cell.CellSize);
            SpriteRenderer oldSprite = cell.GetTransform().GetComponentInChildren<SpriteRenderer>();
            if (oldSprite != null)
            {
                oldSprite.DOKill();
                Object.Destroy(oldSprite);
            }
            SpriteRenderer sprite = UnityEngine.Object.Instantiate(spritePrefab, cell.GetTransform());
            // A cell is only ever assembled with its whole lifespan ahead of it.
            Color color = config.GetDeviationColor(cell.Deviation);
            color.a = config.GetLifeOpacity(1f);
            sprite.color = color;
            cell.GetTransform().DOScale(config.GetCellScale(cell.CellSize, growth), 0.2f);
            return sprite;
        }

        // Fades a cell's sprite toward the opacity of its remaining life (1 = whole lifespan ahead, 0 = dying).
        public void ShowRemainingLife(SpriteRenderer sprite, float remainingLife, float duration)
        {
            if (sprite == null) return;
            // Complete rather than kill, so a Bite's flash cut short here snaps back to the cell's own tint.
            sprite.DOKill(true);
            sprite.DOFade(config.GetLifeOpacity(remainingLife), duration).SetEase(Ease.Linear);
        }

        // Shows a Predator's Bite: the sprite flashes, then settles on its own tint at the opacity of its
        // now-shorter remaining life.
        public void ShowBite(SpriteRenderer sprite, DeviationEnum deviation, float remainingLife)
        {
            if (sprite == null) return;
            sprite.DOKill(true);
            Color settled = config.GetDeviationColor(deviation);
            settled.a = config.GetLifeOpacity(remainingLife);
            Color flash = config.BiteFlashColor;
            flash.a = settled.a;
            sprite.color = flash;
            sprite.DOColor(settled, config.BiteFlashDuration);
        }

        // Resizes a cell to show its growth within its stage, without rebuilding its sprite.
        public void Regrow(IVisualyConfigurable cell, float growth)
        {
            cell.GetTransform().DOKill();
            cell.GetTransform().DOScale(config.GetCellScale(cell.CellSize, growth), 0.2f);
        }
    }
}