using System;

namespace CoreLoop.Interfaces
{
    public interface ISceneLoader
    {
        event Action BattleEnded;
        void LoadNextLevel();
        void LoadGivenLevel(string levelName);
        void LoadMainMenu();
        void LoadCombatScene(string levelName);
        void UnloadCombatScene();
        void ReloadCurrentCombatScene();
        void LoadCreditsScene();
        void LoadCinematicScene(string scene);
        void ResetPlaythrough();
    }
}