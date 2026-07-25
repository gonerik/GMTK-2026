using CoreLoop.Interfaces;
using Zenject;

namespace GameStateMachine.States
{
    public class PlaceDraggedState : State
    {
        private readonly DefaultActions defaultActions;
        
        [Inject]
        public PlaceDraggedState(DefaultActions defaultActions)
        {
            this.defaultActions = defaultActions;
        }
        
        public override void Enter()
        {
            defaultActions.Level.PlaceDragged.Enable();
        }

        public override void Exit()
        {
            defaultActions.Level.PlaceDragged.Disable();
        }
        
        public class Factory : PlaceholderFactory<PlaceDraggedState> { }
    }
}