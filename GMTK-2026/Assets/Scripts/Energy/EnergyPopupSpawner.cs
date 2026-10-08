using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace Energy
{
    // Floats a "+N" / "-N" over every cell that pays out energy, green for a gain and red for a loss.
    // Popups are pooled as children of this object, so keep its scale at 1 and rotation at 0.
    public class EnergyPopupSpawner : MonoBehaviour
    {
        [SerializeField] private EnergyPopup popupPrefab;
        [SerializeField] private Color gainColor = new Color(0.35f, 1f, 0.35f);
        [SerializeField] private Color lossColor = new Color(1f, 0.3f, 0.3f);
        [SerializeField, Tooltip("Offset from the cell's centre, in world units.")]
        private Vector3 spawnOffset = new Vector3(0f, 0.4f, 0f);
        [SerializeField, Min(0f), Tooltip("Random sideways offset, so cells dying together don't stack their numbers.")]
        private float horizontalJitter = 0.3f;

        [Inject] private EnergyService energyService;

        private readonly Stack<EnergyPopup> pool = new Stack<EnergyPopup>();

        private void OnEnable()
        {
            energyService.OnPayout += HandlePayout;
        }

        private void OnDisable()
        {
            energyService.OnPayout -= HandlePayout;
        }

        private void HandlePayout(int amount, Vector3 worldPosition)
        {
            // Cells pay out from OnDestroy, which also runs while the scene unloads; spawning then would
            // leave objects behind after the unload.
            if (amount == 0 || !gameObject.scene.isLoaded) return;

            Vector3 position = worldPosition + spawnOffset
                               + Vector3.right * Random.Range(-horizontalJitter, horizontalJitter);
            EnergyPopup popup = pool.Count > 0 ? pool.Pop() : Instantiate(popupPrefab, transform);
            string label = amount > 0 ? "+" + amount : amount.ToString();
            popup.Play(label, amount > 0 ? gainColor : lossColor, position, Release);
        }

        private void Release(EnergyPopup popup)
        {
            pool.Push(popup);
        }

        [ContextMenu("Test Gain Popup")]
        private void TestGain() => HandlePayout(12, transform.position);

        [ContextMenu("Test Loss Popup")]
        private void TestLoss() => HandlePayout(-5, transform.position);
    }
}
