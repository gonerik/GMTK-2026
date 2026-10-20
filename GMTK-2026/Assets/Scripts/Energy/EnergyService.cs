using System;
using UnityEngine;
using Zenject;

namespace Energy
{
    public class EnergyService : ITickable, IMeter
    {
        [Serializable]
        public class Settings
        {
            // Energy lost per second of game time; fractions carry over between frames. 0 switches decay off.
            [Min(0)] public float decayPerSecond = 1f;
        }

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
        private readonly Settings settings;
        private float decayOwed;
        private bool hasRunOut;
        private const string LoseSound = "event:/Lose";

        public EnergyService(Settings settings)
        {
            this.settings = settings;
            energyAmount = 40;
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

        // The rate is read every frame, so it can be tuned on GameInstaller during Play mode. Time.deltaTime is
        // 0 while paused, so the decay stops with the game.
        public void Tick()
        {
            decayOwed += settings.decayPerSecond * Time.deltaTime;
            if (decayOwed < 1f) return;

            int loss = (int)decayOwed;
            decayOwed -= loss;
            energyAmount = Mathf.Max(energyAmount - loss, 0);
            OnEnergyChanged?.Invoke(energyAmount);
            if (energyAmount > 0 || hasRunOut) return;

            hasRunOut = true;
            FMODUnity.RuntimeManager.PlayOneShot(LoseSound);
            signalBus.Fire(new OnEnergyLostSignal());
        }
    }
}