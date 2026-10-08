using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Energy
{
    public class EnergyService : IDisposable, IMeter
    {
        private const int MaxEnergy = 100;

        public struct OnEnergyGoalReachedSignal
        {
        }
        public struct OnEnergyLostSignal
        {
        }
        
        public event Action<int> OnEnergyChanged;

        public int Current => energyAmount;
        public int Max => MaxEnergy;

        event Action<int> IMeter.OnChanged
        {
            add => OnEnergyChanged += value;
            remove => OnEnergyChanged -= value;
        }

        // A payout that happened somewhere in the world (a cell dying), with the amount the cell paid rather
        // than what the clamp let through. Drives the floating numbers.
        public event Action<int, Vector3> OnPayout;

        
        [Inject] private SignalBus signalBus;
        private int energyAmount;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private const string LoseSound = "event:/Lose";
        
        public EnergyService()
        {
            //LeakEnergy(_cts.Token).Forget();
            energyAmount = 40;
        }

        public void Dispose()
        {
            _cts.Cancel();
            _cts.Dispose();
        }

        public void AddEnergy(int amount)
        {
            energyAmount = (int)MathF.Min(energyAmount + amount, MaxEnergy);
            OnEnergyChanged?.Invoke(energyAmount);
            if (energyAmount >= MaxEnergy)
            {
                signalBus.Fire(new OnEnergyGoalReachedSignal());
            }
        }

        public void AddEnergy(int amount, Vector3 worldPosition)
        {
            AddEnergy(amount);
            OnPayout?.Invoke(amount, worldPosition);
        }

        private async UniTaskVoid LeakEnergy(CancellationToken cancellationToken)
        {
            while (cancellationToken.IsCancellationRequested == false)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: cancellationToken);
                energyAmount -= 1;
                if (energyAmount < 0) energyAmount = 0;
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