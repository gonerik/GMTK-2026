using Interfaces;
using UnityEngine;

namespace DefaultNamespace.Strategies
{
    public class GreenColor : IColor
    {
        public void Initialize(CellUnit cell)
        {
            Debug.Log("Green Color Initialized");
            cell.OnMate += OnMate;
        }

        public void Unsubscribe(CellUnit cell)
        {
            cell.OnMate -= OnMate;
        }

        private void OnMate(IMate partner)
        {
            Debug.Log("Green Cell mated with someone!");
        }
    }

    public class YellowColor : IColor
    {
        public void Initialize(CellUnit cell)
        {
            Debug.Log("Yellow Color Initialized");
            cell.OnMate += OnMate;
        }

        public void Unsubscribe(CellUnit cell)
        {
            cell.OnMate -= OnMate;
        }

        private void OnMate(IMate partner)
        {
            Debug.Log("Yellow Cell mated!");
        }
    }

    public class RedColor : IColor
    {
        public void Initialize(CellUnit cell)
        {
            Debug.Log("Red Color Initialized");
            cell.OnMate += HandleOnMate;
        }

        public void Unsubscribe(CellUnit cell)
        {
            cell.OnMate -= HandleOnMate;
        }

        private void HandleOnMate(IMate partner) => Debug.Log("Red Cell mated!");
    }
}
