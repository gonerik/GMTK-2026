using System;
using Energy;
using UnityEngine.SceneManagement;
using Zenject;

namespace UI
{
    // Ends the run: loads the end scene once EnergyService says a Win or a Defeat has landed.
    public class RunEndLoader : IInitializable, IDisposable
    {
        private const string WinScene = "WinState";
        private const string DefeatScene = "LoseState";

        private readonly SignalBus signalBus;

        public RunEndLoader(SignalBus signalBus)
        {
            this.signalBus = signalBus;
        }

        public void Initialize()
        {
            signalBus.Subscribe<EnergyService.OnWinSignal>(OnWin);
            signalBus.Subscribe<EnergyService.OnDefeatSignal>(OnDefeat);
        }

        public void Dispose()
        {
            signalBus.Unsubscribe<EnergyService.OnWinSignal>(OnWin);
            signalBus.Unsubscribe<EnergyService.OnDefeatSignal>(OnDefeat);
        }

        private void OnWin()
        {
            SceneManager.LoadScene(WinScene);
        }

        private void OnDefeat()
        {
            SceneManager.LoadScene(DefeatScene);
        }
    }
}
