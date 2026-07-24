using Interfaces;
using UnityEngine;

namespace DefaultNamespace.Strategies
{
    public class DefaultMembrane : IMembrane
    {
        public void Initialize(Cell cell)
        {
            Debug.Log("Default Membrane Initialized");
            cell.OnMate += OnMate;
        }

        public void Unsubscribe(Cell cell)
        {
            cell.OnMate -= OnMate;
        }

        private void OnMate(IMate partner) => Debug.Log("Default Membrane: Mated!");
    }

    public class SpikeMembrane : IMembrane
    {
        public void Initialize(Cell cell)
        {
            Debug.Log("Spike Membrane Initialized");
            cell.OnMate += OnMate;
        }

        public void Unsubscribe(Cell cell)
        {
            cell.OnMate -= OnMate;
        }

        private void OnMate(IMate partner) => Debug.Log("Spike Membrane: Mated!");
    }

    public class FluidMembrane : IMembrane
    {
        public void Initialize(Cell cell)
        {
            Debug.Log("Fluid Membrane Initialized");
            cell.OnMate += OnMate;
        }

        public void Unsubscribe(Cell cell)
        {
            cell.OnMate -= OnMate;
        }

        private void OnMate(IMate partner) => Debug.Log("Fluid Membrane: Mated!");
    }
}
