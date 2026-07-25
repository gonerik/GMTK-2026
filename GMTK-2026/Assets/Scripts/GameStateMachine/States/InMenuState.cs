using CoreLoop.Interfaces;
using UnityEngine;
using Zenject;

namespace GameStateMachine.States
{
    public class InMenuState : State
    {
        public override void Enter()
        {
            Time.timeScale = 0f;
        }

        public override void Exit()
        {
            Time.timeScale = 1f;
        }
        
        public class Factory : PlaceholderFactory<InMenuState> { }
    }
}