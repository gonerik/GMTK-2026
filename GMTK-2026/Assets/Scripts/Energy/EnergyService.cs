using System;
using Cysharp.Threading.Tasks;
using Zenject;

namespace Energy
{
    public class EnergyService
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
        
        public EnergyService()
        {
            LeakEnergy().Forget();
            energyAmount = 40;
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
        
        private async UniTaskVoid LeakEnergy()
        {
            while (true)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(1));
                energyAmount -= 1;
                OnEnergyChanged?.Invoke(energyAmount);
                if (energyAmount <= 0)
                {
                    signalBus.Fire(new OnEnergyLostSignal());
                }
            }
        }
    }
}