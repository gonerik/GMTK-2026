using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Zenject;

namespace Energy
{
    public class EnergyService : IDisposable
    {
        public struct OnEnergyGoalReachedSignal
        {
        }
        public struct OnEnergyLostSignal
        {
        }
        
        public event Action<int> OnEnergyChanged;
        
        
        [Inject] private SignalBus signalBus;
        private int energyAmount;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private const string LoseSound = "event:/Lose";
        
        public EnergyService()
        {
            LeakEnergy(_cts.Token).Forget();
            energyAmount = 40;
        }

        public void Dispose()
        {
            _cts.Cancel();
            _cts.Dispose();
        }

        public void AddEnergy(int amount)
        {
            energyAmount += amount;
            OnEnergyChanged?.Invoke(energyAmount);
            if (energyAmount >= 100)
            {
                signalBus.Fire(new OnEnergyGoalReachedSignal());
            }
        }
        
        private async UniTaskVoid LeakEnergy(CancellationToken cancellationToken)
        {
            while (cancellationToken.IsCancellationRequested == false)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: cancellationToken);
                energyAmount -= 1;
                OnEnergyChanged?.Invoke(energyAmount);
                if (energyAmount <= 0)
                {
                    FMODUnity.RuntimeManager.PlayOneShot(LoseSound);
                    signalBus.Fire(new OnEnergyLostSignal());
                }
            }
        }
    }
}