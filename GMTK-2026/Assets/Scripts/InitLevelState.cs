using System;
using CoreLoop.Interfaces;
using GameStateMachine.States;
using UnityEngine;
using Zenject;

namespace DefaultNamespace
{
    public class InitLevelState : MonoBehaviour
    {
        [Inject] private IGameStateMachine gameStateMachine;
        [Inject] private DragState.Factory dragStateFactory;
        
        private void Start()
        {
            gameStateMachine.ChangeState(dragStateFactory.Create());
        }
    }
}