using CoreLoop.Interfaces;
using Zenject;

namespace GameStateMachine.States
{
    public class GameLoopState : State
    {
        [Inject]
        public GameLoopState()
        {
        }

        public override void Enter()
        {
        }

        public override void Exit()
        {
            
        }

        public class Factory : PlaceholderFactory<GameLoopState> { }
    }
}