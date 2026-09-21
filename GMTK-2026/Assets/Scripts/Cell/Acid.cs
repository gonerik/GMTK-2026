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
        // One factory per Acid kind. Both produce this same class from different prefabs; they need
        // distinct types because Zenject keys a factory binding on the factory type.
        public class AgressiveFactory : PlaceholderFactory<Acid>
        {
        }

        public class HornyFactory : PlaceholderFactory<Acid>
        {
        }

        // Every Acid serves exactly one transition. Explicit rather than "strain + 1" so that the
        // ordinal order of MatingEnum is not load-bearing here. The initialisers are the Agressive Acid
        // recipe, so a prefab with no serialized value for these (the original one) behaves as Agressive Acid.
        [Tooltip("The strain a Small cell must have to consume this Acid.")]
        [SerializeField] private MatingEnum consumesStrain = MatingEnum.Default;
        [Tooltip("The strain the cell is promoted to after consuming it.")]
        [SerializeField] private MatingEnum grantsStrain = MatingEnum.Agressive;

        public MatingEnum ConsumesStrain => consumesStrain;
        public MatingEnum GrantsStrain => grantsStrain;

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
                Deviation = DeviationEnum.Default,
                AcidConsumes = consumesStrain
            };
        }
        public void Destroy()
        {
            navigationSystem.UnregisterTarget(this);
            Destroy(gameObject);
        }
    }
}