using System.Threading;
using Cysharp.Threading.Tasks;
using Interfaces;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace Cell.Selectable
{
    public class SelectableController : IInitializable, System.IDisposable
    {
        private DefaultActions defaultActions;
        private ISelectable _currentSelectable;
        private CancellationTokenSource _cts;
        
        public event System.Action<SelectionInfo> OnSelected;
        
        [Inject]
        public SelectableController(DefaultActions defaultActions)
        {
            this.defaultActions = defaultActions;
        }

        public void Initialize()
        {
            defaultActions.Level.Drag.performed += OnDragPerformed;
        }

        public void Dispose()
        {
            defaultActions.Level.Drag.performed -= OnDragPerformed;
            StopSelectionLoop();
        }

        private void OnDragPerformed(InputAction.CallbackContext context)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            Collider2D hit = Physics2D.OverlapCircle(worldPos, 0.1f);

            if (hit != null && hit.TryGetComponent<ISelectable>(out var selectable))
            {
                if (_currentSelectable == selectable) return;
                
                _currentSelectable = selectable;
                OnSelected?.Invoke(selectable.GetSelectionInfo());
                RestartSelectionLoop();
            }
            else
            {
                _currentSelectable = null;
                StopSelectionLoop();
            }
        }

        private void RestartSelectionLoop()
        {
            StopSelectionLoop();
            _cts = new CancellationTokenSource();
            SelectionLoop(_cts.Token).Forget();
        }

        private void StopSelectionLoop()
        {
            OnSelected?.Invoke(new SelectionInfo());
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        private async UniTaskVoid SelectionLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await UniTask.Delay(1000, cancellationToken: token);
                
                if (_currentSelectable != null)
                {
                    OnSelected?.Invoke(_currentSelectable.GetSelectionInfo());
                }
            }
        }
    }
}