using Energy;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI
{
    public class HealthBarTextContrast : MonoBehaviour
    {
        [SerializeField] private Image healthBarFill;       // BarBackground
        [SerializeField] private RectTransform maskFilled;   // TextMask_Filled (bottom)
        [SerializeField] private RectTransform maskEmpty;    // TextMask_Empty (top)
        [SerializeField] private TMP_Text textOnFilled;      // black text
        [SerializeField] private TMP_Text textOnEmpty;       // white text
        [SerializeField] private float containerHeight = 100f;
        [Inject] public EnergyService energyService;
        
        private void Awake()
        {
            //image = GetComponent<Image>();
            energyService.OnEnergyChanged += OnEnergyChanged;
        } 
        public void OnDestroy()
        {
            energyService.OnEnergyChanged -= OnEnergyChanged;
        }

        private void OnEnergyChanged(int percent01)
        {
            string label = percent01 + "%";
            textOnFilled.text = label;
            textOnEmpty.text = label;

            float filledHeight = containerHeight - percent01;

            maskFilled.sizeDelta = new Vector2(maskFilled.sizeDelta.x, containerHeight - filledHeight);
            maskEmpty.sizeDelta = new Vector2(maskEmpty.sizeDelta.x, filledHeight);
        }
    }
}