using System;
using DefaultNamespace;
using MateStrategy;
using UnityEngine;

namespace Interfaces
{
    public interface IVisualyConfigurable
    {
        public event Action<IVisualyConfigurable> OnReinitialized;
        public DeviationEnum Deviation { get; }
        public Transform GetTransform();
        public CellSize CellSize { get; }
        public MatingEnum MatingEnum { get; }
        
    }
}