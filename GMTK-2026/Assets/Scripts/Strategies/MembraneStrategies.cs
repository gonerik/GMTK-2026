using Interfaces;
using UnityEngine;

namespace DefaultNamespace.Strategies
{
    public class DefaultMembrane : IMembrane
    {
        public void Initialize(Cell cell)
        {
            Debug.Log("Default Membrane Initialized");
            cell.OnEat += OnEat;
        }

        public void Unsubscribe(Cell cell)
        {
            cell.OnEat -= OnEat;
        }

        private void OnEat(IConsumer consumer) => Debug.Log("Default Membrane: I'm being eaten!");
    }

    public class SpikeMembrane : IMembrane
    {
        public void Initialize(Cell cell)
        {
            Debug.Log("Spike Membrane Initialized");
            cell.OnEat += OnEat;
        }

        public void Unsubscribe(Cell cell)
        {
            cell.OnEat -= OnEat;
        }

        private void OnEat(IConsumer consumer) => Debug.Log("Spike Membrane: Ouch! You ate spikes!");
    }

    public class FluidMembrane : IMembrane
    {
        public void Initialize(Cell cell)
        {
            Debug.Log("Fluid Membrane Initialized");
            cell.OnConsume += OnConsume;
        }

        public void Unsubscribe(Cell cell)
        {
            cell.OnConsume -= OnConsume;
        }

        private void OnConsume(IEatable eatable) => Debug.Log("Fluid Membrane: Slorp!");
    }
}
