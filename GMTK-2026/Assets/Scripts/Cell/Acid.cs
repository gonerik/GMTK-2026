using System;
using System.Collections.Generic;
using DefaultNamespace.Zenject;
using Interfaces;
using MateStrategy;
using UnityEngine;
using Zenject;

namespace DefaultNamespace
{
    public class Acid : MonoBehaviour, IMate
    {
        public class Factory : PlaceholderFactory<Acid>
        {
        }

        public CellSize CellSize => CellSize.Acid;
        public float DetectionRange => 0f;
        
        public int EnergyAmount => 0;
        public bool IsMating
        {
            get;
            set;
        }

        [Inject] private NavigationSystem navigationSystem;
        [Inject] private MatingService matingService;

        private void Start()
        {
            navigationSystem.RegisterTarget(this);
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.TryGetComponent(out IMate partner))
            {
                if (IsMating || partner.IsMating)
                {
                    return;
                }
                matingService.Mate(this, partner);
            }
        }

        public MatingEnum GetMatingEnum()
        {
            return MatingEnum.Acid;
        }

        public void Mate(IMate mate)
        {
        }

        public bool CanTarget(ITarget target)
        {
            return false;
        }

        public Vector3 GetTargetPosition()
        {
            return transform.position;
        }

        public void AddTargetingRule(Predicate<AIView> predicate)
        {
        }

        public AIView GetView()
        {
            return new AIView()
            {
                CellSize = CellSize,
                MatingEnum = GetMatingEnum(),
                Deviation = DeviationEnum.Default
            };
        }
        public void Destroy()
        {
            navigationSystem.UnregisterTarget(this);
            Destroy(gameObject);
        }
    }
}