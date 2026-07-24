using System;
using DefaultNamespace;
using MateStrategy;

namespace Interfaces
{
    public interface IMate : IEntity, ITarget
    {
        public bool IsMating { get; set; }
        public MatingEnum GetMatingEnum();
    }
}
