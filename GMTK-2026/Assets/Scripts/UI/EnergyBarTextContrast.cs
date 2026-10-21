using DG.Tweening;
using Energy;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI
{
    // Labels a bar with its meter's percentage. Despite the name it shows Food too (see meter). On the Energy
    // bar it also pulses the bar's background red while Energy is in Grace.
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
        [SerializeField, Tooltip("Pulses while Energy is in Grace. Left empty, it is this object's own Graphic (the bar's background).")]
        private Graphic graceWarning;
        [SerializeField] private Color graceColor = Color.red;
        [SerializeField, Min(0.01f), Tooltip("Seconds for each half of the Grace pulse.")]
        private float graceHalfPulse = 0.25f;
        [Inject] public EnergyService energyService;
        [Inject] private FoodService foodService;

        private IMeter source;
        private Color graceWarningColor;

        private void Awake()
        {
            //image = GetComponent<Image>();
            source = meter == MeterKind.Food ? foodService : energyService;
            source.OnChanged += OnMeterChanged;

            if (meter != MeterKind.Energy) return;
            if (graceWarning == null) graceWarning = GetComponent<Graphic>();
            if (graceWarning == null) return;

            graceWarningColor = graceWarning.color;
            energyService.OnGraceChanged += OnGraceChanged;
        }

        // Shows the starting amount rather than whatever label the scene was saved with.
        private void Start()
        {
            OnMeterChanged(source.Current);
        }

        public void OnDestroy()
        {
            source.OnChanged -= OnMeterChanged;
            energyService.OnGraceChanged -= OnGraceChanged;
            if (graceWarning != null) graceWarning.DOKill();
        }

        // Scaled time, so the pulse freezes with the Grace countdown while the game is paused.
        private void OnGraceChanged(bool inGrace)
        {
            graceWarning.DOKill();
            if (inGrace)
            {
                graceWarning.DOColor(graceColor, graceHalfPulse).SetLoops(-1, LoopType.Yoyo);
            }
            else
            {
                graceWarning.color = graceWarningColor;
            }
        }

        private void OnMeterChanged(int amount)
        {
            float fill = (float)amount / source.Max;
            string label = Mathf.RoundToInt(fill * 100f) + "%";
            textOnFilled.text = label;
            textOnEmpty.text = label;

            // Both meters stay within 0..Max today; the clamp keeps the masks inside the container regardless.
            float clampedFill = Mathf.Clamp01(fill);
            maskFilled.sizeDelta = new Vector2(maskFilled.sizeDelta.x, containerHeight * clampedFill);
            maskEmpty.sizeDelta = new Vector2(maskEmpty.sizeDelta.x, containerHeight * (1f - clampedFill));
        }
    }
}