using System;
using System.Collections.Generic;
using DefaultNamespace.GrowStrategy;
using DefaultNamespace.Zenject;
using Interfaces;
using UnityEngine;
using Zenject;

namespace DefaultNamespace
{
    public class Acid : MonoBehaviour, IEatable
    {
        [SerializeField] private GrowStrategyEnum growStrategy;
        public CellSize CellSize => CellSize.Acid;
        
        [Inject] private NavigationSystem navigationSystem;

        private void Start()
        {
            navigationSystem.RegisterEatable(this);
        }

        private void OnDestroy()
        {
            navigationSystem.UnregisterEatable(this);
        }
		
        public virtual IGrowStrategy Eat(IConsumer consumer)
        {
            Destroy(gameObject);
            return CellStrategyFactory.CreateGrowStrategy(growStrategy);
        }

        public void Destroy()
        {
            Destroy(gameObject);
        }
    }
}