using CoreLoop.Interfaces;
using Energy;
using GameStateMachine.States;
using UnityEngine;
using Zenject;

namespace DefaultNamespace
{
    // Shows the Food tutorial once per run, the first time Food falls to a share of its cap. It waits until the
    // player's hands are free - no cell held, syringe not armed - and never shows once the Win has landed.
    public class FoodTutorialTrigger : MonoBehaviour
    {
        [Tooltip("The Food tutorial, inactive in the scene until it is owed.")]
        [SerializeField] private TutorialAnim tutorial;

        [Tooltip("Share of the Food cap (0..1) at or below which the tutorial is owed.")]
        [SerializeField, Range(0f, 1f)] private float foodShare = 0.3f;

        [Tooltip("Seconds the tutorial ignores presses after it appears. It pops up mid-game, so a press meant for the board would otherwise skip it unread.")]
        [SerializeField, Min(0f)] private float dismissDelay = 1.5f;

        [Inject] private FoodService foodService;
        [Inject] private EnergyService energyService;
        [Inject] private IGameStateMachine gameStateMachine;
        [Inject] private TutorialGate tutorialGate;

        // Once owed it stays owed, even if Food climbs back above the share before it can show.
        private bool owed;
        private bool shown;

        private void OnEnable()
        {
            foodService.OnChanged += HandleFoodChanged;
        }

        private void OnDisable()
        {
            foodService.OnChanged -= HandleFoodChanged;
        }

        private void HandleFoodChanged(int food)
        {
            if (food <= foodService.Max * foodShare) owed = true;
        }

        private void Update()
        {
            if (!owed || shown || tutorial == null) return;

            // Energy at its cap is a Win: the run is decided, and nothing interrupts the beat before it ends.
            if (energyService.Current >= energyService.Max) return;

            // Only DragState listens to the press that hides a tutorial. Anywhere else - a cell in hand, the
            // syringe armed, the pause menu - it could not be dismissed, so wait.
            if (tutorialGate.IsShowing || gameStateMachine.CurrentState is not DragState) return;

            shown = true;
            tutorial.gameObject.SetActive(true);
            tutorial.BlockDismissFor(dismissDelay);
        }
    }
}
