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
            // Seconds of game time Energy may sit empty (Grace) before the run ends in Defeat. Any gain ends it.
            [Min(0)] public float graceSeconds = 2f;
            // Seconds of game time between reaching the cap and the Win landing. The meter is locked meanwhile.
            [Min(0)] public float winBeatSeconds = 2f;
        }

        private const int MaxEnergy = 100;

        // Each fires once, when the run's outcome lands: after the Win beat, or when a Grace runs out.
        public struct OnWinSignal
        {
        }
        public struct OnDefeatSignal
        {
        }

        public event Action<int> OnEnergyChanged;

        // True when Energy empties and Grace starts, false when a gain lifts it off empty.
        public event Action<bool> OnGraceChanged;

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
        private bool inGrace;
        private float graceElapsed;
        // Reaching the cap locks the meter: nothing changes Energy until the Win lands.
        private bool isWon;
        private float winBeatElapsed;
        // A Win or a Defeat has landed, so the run is over.
        private bool isOver;
        private const string LoseSound = "event:/Lose";
        private const string WinSound = "event:/Win";

        public EnergyService(Settings settings)
        {
            this.settings = settings;
            energyAmount = 45;
        }

        public void AddEnergy(int amount)
        {
            if (isWon || isOver) return;
            SetEnergy(energyAmount + amount);
        }

        public void AddEnergy(int amount, Vector3 worldPosition)
        {
            // A locked meter shows no popups either, so nothing contradicts a Win's full bar.
            if (isWon || isOver) return;
            AddEnergy(amount);
            OnPayout?.Invoke(amount, worldPosition);
        }

        // The settings are read every frame, so they can be tuned on GameInstaller during Play mode.
        // Time.deltaTime is 0 while paused, so decay, Grace and the Win beat all stop with the game.
        public void Tick()
        {
            if (isOver) return;

            if (isWon)
            {
                winBeatElapsed += Time.deltaTime;
                if (winBeatElapsed < settings.winBeatSeconds) return;

                isOver = true;
                signalBus.Fire(new OnWinSignal());
                return;
            }

            decayOwed += settings.decayPerSecond * Time.deltaTime;
            if (decayOwed >= 1f)
            {
                int loss = (int)decayOwed;
                decayOwed -= loss;
                SetEnergy(energyAmount - loss);
            }

            // Counted here rather than off decay, so a penalty that empties Energy still ends the run with decay
            // switched off.
            if (!inGrace) return;
            graceElapsed += Time.deltaTime;
            if (graceElapsed < settings.graceSeconds) return;

            isOver = true;
            FMODUnity.RuntimeManager.PlayOneShot(LoseSound);
            signalBus.Fire(new OnDefeatSignal());
        }

        // Every change goes through here: Energy never leaves 0..MaxEnergy, reaching the cap is a Win, and
        // emptying starts Grace.
        private void SetEnergy(int amount)
        {
            energyAmount = Mathf.Clamp(amount, 0, MaxEnergy);
            OnEnergyChanged?.Invoke(energyAmount);

            if (energyAmount >= MaxEnergy)
            {
                isWon = true;
                SetGrace(false);
                FMODUnity.RuntimeManager.PlayOneShot(WinSound);
                return;
            }

            SetGrace(energyAmount == 0);
        }

        // A no-op when nothing changes, so a decay tick at 0 never restarts the countdown.
        private void SetGrace(bool active)
        {
            if (inGrace == active) return;

            inGrace = active;
            graceElapsed = 0f;
            OnGraceChanged?.Invoke(active);
        }
    }
}