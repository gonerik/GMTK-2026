using System;
using System.Collections.Generic;
using System.Threading;
using Cell;
using Cell.Selectable;
using Cysharp.Threading.Tasks;
using MateStrategy;
using UI.Selectable;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI.Reel
{
    /// <summary>
    /// Owns the strain list and current index, and turns button clicks into seed strain changes,
    /// announced with SeedStrainChangedSignal. An Undiscovered strain can be landed on but not picked:
    /// it shows locked for a moment, then the reel snaps back to the seed strain. Delegates all
    /// presentation to a ReelView, so this class stays free of animation concerns.
    /// </summary>
    public class ReelSelectorController : MonoBehaviour
    {
        /// <summary>Fired when the player picks a Discovered strain as the new seed strain.</summary>
        public readonly struct SeedStrainChangedSignal
        {
            public readonly MatingEnum Strain;

            public SeedStrainChangedSignal(MatingEnum strain)
            {
                Strain = strain;
            }
        }

        [Header("Options")]
        [SerializeField, Tooltip("Strains to cycle through, top of list = index 0. Keep ladder order — Default, Agressive, Horny — so Ordinary, the only strain Discovered from the start, comes first. Acid has no icon.")]
        private List<MatingEnum> _options = new List<MatingEnum>();

        [SerializeField, Tooltip("If true, going past the last option wraps to the first (and vice versa).")]
        private bool _wrapAround = true;

        [SerializeField, Min(0f), Tooltip("Seconds an Undiscovered strain stays on display before the reel snaps back to the seed strain.")]
        private float _lockedTeaseDuration = 0.6f;

        [Header("References")]
        [SerializeField, Tooltip("The view responsible for displaying the current option's icon.")]
        private ReelView _view;

        [SerializeField, Tooltip("Advances to the next option.")]
        private Button _nextButton;

        [SerializeField, Tooltip("Goes back to the previous option.")]
        private Button _previousButton;

        [Inject] private SignalBus _signalBus;
        [Inject] private IconInstaller _iconInstaller;
        [Inject] private StrainDiscovery _strainDiscovery;

        private int _seedIndex;
        private ReelDirection _lastDirection;
        private CancellationTokenSource _teaseCts;

        /// <summary>The option on display. Differs from the seed strain only while an Undiscovered strain is shown.</summary>
        public int CurrentIndex { get; private set; }

        private void Awake()
        {
            _nextButton.onClick.AddListener(SelectNext);
            _previousButton.onClick.AddListener(SelectPrevious);
        }

        private void OnDestroy()
        {
            _nextButton.onClick.RemoveListener(SelectNext);
            _previousButton.onClick.RemoveListener(SelectPrevious);
            StopTease();
        }

        private void OnEnable()
        {
            _signalBus.Subscribe<StrainDiscovery.StrainDiscoveredSignal>(OnStrainDiscovered);
        }

        private void OnDisable()
        {
            _signalBus.Unsubscribe<StrainDiscovery.StrainDiscoveredSignal>(OnStrainDiscovered);
        }

        private void Start()
        {
            if (_options.Count == 0) return;

            if (!IsDiscoveredAt(0))
            {
                Debug.LogWarning($"[ReelSelectorController] First option is {_options[0]}, which starts Undiscovered. Put Default (Ordinary) first.", this);
            }

            _view.SetImmediate(IconAt(CurrentIndex), !IsDiscoveredAt(CurrentIndex));
            RefreshButtonInteractability();
        }

        public void SelectNext() => Select(CurrentIndex + 1, ReelDirection.Next);

        public void SelectPrevious() => Select(CurrentIndex - 1, ReelDirection.Previous);

        /// <summary>Jumps straight to an index with no animation (e.g. when loading a saved setting). Undiscovered strains are ignored.</summary>
        public void SetIndexImmediate(int index)
        {
            if (_options.Count == 0) return;

            int clamped = Mathf.Clamp(index, 0, _options.Count - 1);
            if (!IsDiscoveredAt(clamped)) return;

            StopTease();
            CurrentIndex = clamped;
            _view.SetImmediate(IconAt(CurrentIndex), false);
            RefreshButtonInteractability();
            MakeSeed(CurrentIndex);
        }

        private void Select(int rawIndex, ReelDirection direction)
        {
            if (_options.Count == 0) return;

            int newIndex = _wrapAround
                ? ((rawIndex % _options.Count) + _options.Count) % _options.Count
                : Mathf.Clamp(rawIndex, 0, _options.Count - 1);

            if (newIndex == CurrentIndex) return;

            CurrentIndex = newIndex;
            _lastDirection = direction;
            bool discovered = IsDiscoveredAt(CurrentIndex);
            _view.PlayTransition(IconAt(CurrentIndex), direction, !discovered);
            RefreshButtonInteractability();

            if (discovered)
            {
                StopTease();
                MakeSeed(CurrentIndex);
            }
            else
            {
                TeaseThenSnapBack().Forget();
            }
        }

        // Each landing on an Undiscovered strain restarts the wait, so the player can keep cycling
        // through locked options; landing on a Discovered one stops it.
        private async UniTaskVoid TeaseThenSnapBack()
        {
            StopTease();
            _teaseCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());

            bool cancelled = await UniTask.Delay(TimeSpan.FromSeconds(_lockedTeaseDuration), cancellationToken: _teaseCts.Token)
                .SuppressCancellationThrow();
            if (cancelled) return;

            StopTease();
            ReelDirection back = _lastDirection == ReelDirection.Next ? ReelDirection.Previous : ReelDirection.Next;
            CurrentIndex = _seedIndex;
            _view.PlayTransition(IconAt(CurrentIndex), back, false);
            RefreshButtonInteractability();
        }

        private void StopTease()
        {
            if (_teaseCts == null) return;

            _teaseCts.Cancel();
            _teaseCts.Dispose();
            _teaseCts = null;
        }

        private void OnStrainDiscovered(StrainDiscovery.StrainDiscoveredSignal signal)
        {
            _view.Pulse();

            // The player is looking at the strain that was just Discovered: unlock it in place and take it.
            if (_options.Count == 0 || _options[CurrentIndex] != signal.Strain) return;

            StopTease();
            _view.SetLocked(false);
            MakeSeed(CurrentIndex);
        }

        private void MakeSeed(int index)
        {
            if (index == _seedIndex) return;

            _seedIndex = index;
            _signalBus.Fire(new SeedStrainChangedSignal(_options[index]));
        }

        private bool IsDiscoveredAt(int index) => _strainDiscovery.IsDiscovered(_options[index]);

        private Sprite IconAt(int index) => _iconInstaller.GetMatingIcon(_options[index]);

        private void RefreshButtonInteractability()
        {
            if (_wrapAround) return; // both buttons stay usable when wrapping

            _previousButton.interactable = CurrentIndex > 0;
            _nextButton.interactable = CurrentIndex < _options.Count - 1;
        }
    }
}
