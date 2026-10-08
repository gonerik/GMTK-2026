using Energy;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI
{
    // Labels a bar with its meter's percentage. Despite the name it shows Food too (see meter).
    public class EnergyBarTextContrast : MonoBehaviour
    {
        [SerializeField, Tooltip("Which board amount this bar shows.")]
        private MeterKind meter = MeterKind.Energy;
        [SerializeField] private Image healthBarFill;       // BarBackground
        [SerializeField] private RectTransform maskFilled;   // TextMask_Filled (bottom)
        [SerializeField] private RectTransform maskEmpty;    // TextMask_Empty (top)
        [SerializeField] private TMP_Text textOnFilled;      // black text
        [SerializeField] private TMP_Text textOnEmpty;       // white text
        [SerializeField] private float containerHeight = 100f;
        [Inject] public EnergyService energyService;
        [Inject] private FoodService foodService;

        private IMeter source;

        private void Awake()
        {
            //image = GetComponent<Image>();
            source = meter == MeterKind.Food ? foodService : energyService;
            source.OnChanged += OnMeterChanged;
        }

        // Shows the starting amount rather than whatever label the scene was saved with.
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
            float fill = (float)amount / source.Max;
            string label = Mathf.RoundToInt(fill * 100f) + "%";
            textOnFilled.text = label;
            textOnEmpty.text = label;

            // Energy has no floor, so keep the masks inside the container even when the label goes negative.
            float clampedFill = Mathf.Clamp01(fill);
            maskFilled.sizeDelta = new Vector2(maskFilled.sizeDelta.x, containerHeight * clampedFill);
            maskEmpty.sizeDelta = new Vector2(maskEmpty.sizeDelta.x, containerHeight * (1f - clampedFill));
        }
    }
}