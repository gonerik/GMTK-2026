using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Energy
{
    public class EnergyBarUI : MonoBehaviour
    {
        [Inject] public EnergyService energyService;
        
        private Image image;
        
        private void Awake()
        {
            image = GetComponent<Image>();
            energyService.OnEnergyChanged += OnEnergyChanged;
        }
        
        private void OnEnergyChanged(int energyAmount)
        {
            image.fillAmount = energyAmount / 100f;
        }
    }
}