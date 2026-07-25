using CoreLoop.Interfaces;
using GameStateMachine.States;
using Zenject;

namespace CoreLoop
{
    public class GameStateMachineInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindFactory<DragState, DragState.Factory>();
            Container.BindFactory<PlaceDraggedState, PlaceDraggedState.Factory>();
            Container.BindFactory<PlacingBasicCellState, PlacingBasicCellState.Factory>();
            Container.BindFactory<GameLoopState, GameLoopState.Factory>();
            Container.BindFactory<InMenuState, InMenuState.Factory>();
            
            Container.Bind<IGameStateMachine>().To<GameStateMachine>().AsSingle().NonLazy();
        }
    }
}