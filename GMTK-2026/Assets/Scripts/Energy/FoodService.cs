using System;
using UnityEngine;

namespace Energy
{
    // The board's Food: earned when cells die in the Food dish, spent by Seeding. Never below 0 or above Max.
    public class FoodService : IMeter
    {
        public event Action<int> OnChanged;

        // A Payout that landed as Food somewhere in the world, with the amount the cell paid rather than what
        // the clamp let through. Drives the floating numbers.
        public event Action<int, Vector3> OnPayout;

        public int Current { get; private set; }
        public int Max { get; }

        public FoodService(FoodConfig config)
        {
            Max = config.MaxFood;
            Current = Mathf.Clamp(config.StartingFood, 0, Max);
        }

        public void AddFood(int amount, Vector3 worldPosition)
        {
            Current = Mathf.Clamp(Current + amount, 0, Max);
            OnChanged?.Invoke(Current);
            OnPayout?.Invoke(amount, worldPosition);
        }

        // Spends the amount only if the board holds all of it.
        public bool TrySpend(int amount)
        {
            if (Current < amount) return false;
            Current -= amount;
            OnChanged?.Invoke(Current);
            return true;
        }
    }
}
