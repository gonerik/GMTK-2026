using System;
using DefaultNamespace;
using Energy;
using Interfaces;
using MateStrategy;
using TMPro;
using UI.Selectable;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Zenject;

namespace Cell.Selectable
{
    public class SelectableUI : MonoBehaviour
    {
        [Inject] private SelectableController selectableController;
        [Inject] private IconInstaller iconInstaller;
        
        [SerializeField] private TextMeshProUGUI LifetimeText;
        [SerializeField] private TextMeshProUGUI EnergyText;
        [SerializeField] private TextMeshProUGUI SpeedText;
        [SerializeField] private GameObject TraitContainer;
        
        private void Start()
        {
            selectableController.OnSelected += OnSelected;
        }
        
        private void OnSelected(SelectionInfo selectionInfo)
        {
            foreach (Transform child in TraitContainer.transform)
            {
                Destroy(child.gameObject);
            }

            LifetimeText.text = $"Age: {selectionInfo.Age} / {selectionInfo.MaxAge}";
            EnergyText.text = PayoutText(selectionInfo);
            SpeedText.text = $"Speed: {selectionInfo.speed}";
            
            if (selectionInfo.HasPaired)
            {
                LifetimeText.text += "  (spent)";
            }

            if (selectionInfo.MatingEnum != MatingEnum.Acid)
            {
                var image = new GameObject("MatingIcon").AddComponent<Image>();
                image.transform.SetParent(TraitContainer.transform);
                image.sprite = iconInstaller.GetMatingIcon(selectionInfo.MatingEnum);
            }

            if (selectionInfo.Deviation != DeviationEnum.Default)
            {
                var image = new GameObject("DeviationIcon").AddComponent<Image>();
                image.transform.SetParent(TraitContainer.transform);
                image.sprite = iconInstaller.GetDeviationIcon(selectionInfo.Deviation);
            }

            if (selectionInfo.CellSize != CellSize.Acid)
            {
                var image = new GameObject("SizeIcon").AddComponent<Image>();
                image.transform.SetParent(TraitContainer.transform);
                image.sprite = iconInstaller.GetSizeIcon(selectionInfo.CellSize);
            }


        }

        // What the cell's death will pay where it is now. A loss always costs Energy, whatever the dish.
        private static string PayoutText(SelectionInfo selectionInfo)
        {
            if (selectionInfo.EnergyAmount < 0 || selectionInfo.Payout == DishPayout.Energy)
            {
                return $"Energy Gain: {selectionInfo.EnergyAmount}";
            }

            return selectionInfo.Payout == DishPayout.Food ? $"Food Gain: {selectionInfo.FoodAmount}" : "Gain: none";
        }
    }
}