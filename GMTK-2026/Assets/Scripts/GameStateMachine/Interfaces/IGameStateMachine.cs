using System;

namespace CoreLoop.Interfaces
{
    public interface IGameStateMachine
    {
        State CurrentState { get; }
        void ChangeState(State state);
        void ChangeState<TPayload>(State<TPayload> state, TPayload payload) where TPayload : IStatePayload;
        event Action<State> OnStateChanged;
        bool IsInMainMenu { get; }
    }
}