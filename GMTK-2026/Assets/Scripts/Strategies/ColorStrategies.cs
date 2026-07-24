using Interfaces;
using UnityEngine;

namespace DefaultNamespace.Strategies
{
    public class GreenColor : IColor
    {
        public void Initialize(Cell cell)
        {
            Debug.Log("Green Color Initialized");
            cell.OnConsume += OnConsume;
        }

        public void Unsubscribe(Cell cell)
        {
            cell.OnConsume -= OnConsume;
        }

        private void OnConsume(IEatable eatable)
        {
            Debug.Log("Green Cell consumed something!");
            eatable.Destroy();
        }
    }

    public class YellowColor : IColor
    {
        public void Initialize(Cell cell)
        {
            Debug.Log("Yellow Color Initialized");
            cell.OnEat += OnEat;
        }

        public void Unsubscribe(Cell cell)
        {
            cell.OnEat -= OnEat;
        }

        private void OnEat(IConsumer consumer)
        {
            Debug.Log("Yellow Cell was eaten!");
        }
    }

    public class RedColor : IColor
    {
        public void Initialize(Cell cell)
        {
            Debug.Log("Red Color Initialized");
            cell.OnConsume += HandleOnConsume;
        }

        public void Unsubscribe(Cell cell)
        {
            cell.OnConsume -= HandleOnConsume;
        }

        private void HandleOnConsume(IEatable eatable) => Debug.Log("Red Cell consumed something!");
    }
}
