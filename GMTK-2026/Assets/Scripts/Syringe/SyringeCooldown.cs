using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace DefaultNamespace.Syringe
{
    public class SyringeCooldown : MonoBehaviour
    {
        [SerializeField] private Image cooldownImage;
        private bool _isCooldown;

        public bool IsCooldown => _isCooldown;

        private void Awake()
        {
            if (cooldownImage == null)
            {
                cooldownImage = GetComponent<Image>();
            }

            if (cooldownImage != null)
            {
                cooldownImage.type = Image.Type.Filled;
                cooldownImage.fillMethod = Image.FillMethod.Radial360;
                cooldownImage.fillAmount = 0;
                cooldownImage.enabled = false;
            }
        }

        public void StartCooldown(float duration)
        {
            if (_isCooldown) return;
            CooldownRoutine(duration).Forget();
        }

        private async UniTaskVoid CooldownRoutine(float duration)
        {
            _isCooldown = true;
            float elapsed = 0;

            if (cooldownImage != null)
            {
                cooldownImage.enabled = true;
                cooldownImage.fillAmount = 1;
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    cooldownImage.fillAmount = 1 - (elapsed / duration);
                    await UniTask.Yield(PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy());
                }
                cooldownImage.fillAmount = 0;
                cooldownImage.enabled = false;
            }
            else
            {
                await UniTask.Delay(System.TimeSpan.FromSeconds(duration), cancellationToken: this.GetCancellationTokenOnDestroy());
            }

            _isCooldown = false;
        }
    }
}