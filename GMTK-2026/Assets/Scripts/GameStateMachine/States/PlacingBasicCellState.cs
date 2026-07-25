using CoreLoop.Interfaces;
using Zenject;

namespace GameStateMachine.States
{
    public class PlacingBasicCellState : State
    {
        private readonly DefaultActions defaultActions;
        
        [Inject]
        public PlacingBasicCellState(DefaultActions defaultActions)
        {
            this.defaultActions = defaultActions;
        }
        
        public override void Enter()
        {
            defaultActions.Level.PlaceDefaultCell.Enable();
        }

        public override void Exit()
        {
            defaultActions.Level.PlaceDefaultCell.Disable();
        }
        
        public class Factory : PlaceholderFactory<PlacingBasicCellState> { }
    }
}