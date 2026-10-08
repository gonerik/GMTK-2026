using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Energy
{
    // Fills this Image to the share of its meter's maximum. Despite the name it shows Food too (see meter).
    public class EnergyBarUI : MonoBehaviour
    {
        [SerializeField, Tooltip("Which board amount this bar shows.")]
        private MeterKind meter = MeterKind.Energy;

        [Inject] public EnergyService energyService;
        [Inject] private FoodService foodService;

        private Image image;
        private IMeter source;

        private void Awake()
        {
            image = GetComponent<Image>();
            source = meter == MeterKind.Food ? foodService : energyService;
            source.OnChanged += OnMeterChanged;
        }

        // Shows the starting amount rather than whatever fill the scene was saved with.
        private void Start()
        {
            OnMeterChanged(source.Current);
        }

        public void OnDestroy()
        {
            source.OnChanged -= OnMeterChanged;
        }

        private void OnMeterChanged(int amount)
        {
            image.fillAmount = (float)amount / source.Max;
        }
    }
}