using Interfaces;
using UnityEngine;

namespace DefaultNamespace.Strategies
{
    public class GreenColor : IColor
    {
        public void Initialize(Cell cell)
        {
            Debug.Log("Green Color Initialized");
        }
    }

    public class YellowColor : IColor
    {
        public void Initialize(Cell cell)
        {
            Debug.Log("Yellow Color Initialized");
        }
    }

    public class RedColor : IColor
    {
        public void Initialize(Cell cell)
        {
            Debug.Log("Red Color Initialized");
        }
    }
}
