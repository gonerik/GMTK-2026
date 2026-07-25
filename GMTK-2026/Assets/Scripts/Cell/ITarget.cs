using System;
using DefaultNamespace;
using UnityEngine;

namespace DefaultNamespace
{
    public interface ITarget
    {
        public float DetectionRange { get; }
        public bool CanTarget(ITarget target);
        public Vector3 GetTargetPosition();
        public int EnergyAmount { get; }
        public void AddTargetingRule(Predicate<AIView> predicate);
        
        public AIView GetView();
    }
}