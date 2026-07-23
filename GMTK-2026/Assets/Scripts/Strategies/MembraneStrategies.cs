using Interfaces;
using UnityEngine;

namespace DefaultNamespace.Strategies
{
    public class DefaultMembrane : IMembrane
    {
        public void Initialize(Cell cell)
        {
            Debug.Log("Default Membrane Initialized");
        }
    }

    public class SpikeMembrane : IMembrane
    {
        public void Initialize(Cell cell)
        {
            Debug.Log("Spike Membrane Initialized");
        }
    }

    public class FluidMembrane : IMembrane
    {
        public void Initialize(Cell cell)
        {
            Debug.Log("Fluid Membrane Initialized");
        }
    }
}
