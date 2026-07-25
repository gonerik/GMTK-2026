using System;
using CoreLoop.Interfaces;

namespace CoreLoop
{
    public class GameStateMachine : IGameStateMachine
    {
        public State CurrentState => currentState;
        private State currentState;
        public event Action<State> OnStateChanged;
        public bool IsInMainMenu { get; }


        public GameStateMachine()
        {
        }
        
        public void ChangeState(State state)
        {
            if (currentState != null && currentState.GetType() == state.GetType()) return;
            currentState?.Exit();
            currentState = state;
            currentState.Enter();
            OnStateChanged?.Invoke(state);
        }

        public void ChangeState<TPayload>(State<TPayload> state, TPayload payload) where TPayload : IStatePayload
        {
            state.Payload = payload;
            if (currentState != null && currentState.GetType() == state.GetType())
            {
                var currentWithPayload = currentState as State<TPayload>;
                if (currentWithPayload != null && Equals(currentWithPayload.Payload, payload))
                {
                    return;
                }
            }
            
            currentState?.Exit();
            currentState = state;
            currentState.Enter();
            OnStateChanged?.Invoke(state);
        }
    }
}