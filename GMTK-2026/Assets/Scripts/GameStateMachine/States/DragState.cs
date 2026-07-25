using CoreLoop.Interfaces;
using Zenject;

namespace GameStateMachine.States
{
    public class DragState : State
    {
        private readonly DefaultActions defaultActions;
        
        [Inject]
        public DragState(DefaultActions defaultActions)
        {
            this.defaultActions = defaultActions;
        }
        
        public override void Enter()
        {
            defaultActions.Level.Drag.Enable();
        }

        public override void Exit()
        {
            defaultActions.Level.Drag.Disable();
        }
        
        public class Factory : PlaceholderFactory<DragState> { }
    }
}