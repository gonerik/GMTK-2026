using System;
using System.Collections.Generic;
using Cell.Visual;
using DefaultNamespace.Zenject;
using Energy;
using Interfaces;
using MateStrategy;
using UnityEngine;
using Zenject;

namespace DefaultNamespace
{
    public class RedCell : MonoBehaviour, IVisualyConfigurable, ITarget, IEntity
    {
        [SerializeField] private CellSize cellSize;
        [SerializeField] private MatingEnum matingEnum;
        [SerializeField] private int energyAmount;
        [SerializeField] private float moveSpeed;
        
        public event Action<IVisualyConfigurable> OnReinitialized;
        public DeviationEnum Deviation => DeviationEnum.Red;
        
        [Inject] private NavigationSystem navigationSystem;
        [Inject] private EnergyService energyService;
        [Inject] private CellVisualAssembler visualAssembler;
        private List<Predicate<AIView>> targetingRules = new List<Predicate<AIView>>();
        private Rigidbody2D rb;
        private float wanderTimer;
        private bool isWandering;
        private Vector2 currentWanderDirection;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            visualAssembler.Reassemble(this);
        }

        public Transform GetTransform()
        {
            return transform;
        }

        public CellSize CellSize => cellSize;
        public void Destroy()
        {
            energyService.AddEnergy(energyAmount);
            navigationSystem.UnregisterTarget(this);
            Destroy(gameObject);
        }

        public MatingEnum MatingEnum => matingEnum;
        public float DetectionRange => 10f;
        public int EnergyAmount => energyAmount;
        
        public bool CanTarget(ITarget target)
        {
            bool canTarget = true;
            foreach (var rule in targetingRules)
            {
                if (!rule(target.GetView()))
                {
                    canTarget = false;
                    break;
                }
            }
            return canTarget;
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.TryGetComponent(out CellUnit cell))
            {
                if (cell.CellSize == cellSize && !cell.IsMating)
                {
                    cell.Destroy();
                }
            }
        }

        void FixedUpdate()
        {
            Vector2 targetDirection = Vector2.zero;
            bool shouldMove = false;

            if (navigationSystem.targets.TryGetValue(this, out var target))
            {
                Vector2 toTarget = (Vector2)target.GetTargetPosition() - (Vector2)transform.position;
                targetDirection = toTarget.normalized;
                shouldMove = true;
            }
            else
            {
                if (HandleWandering(out var wanderDir))
                {
                    targetDirection = wanderDir;
                    shouldMove = true;
                }
            }

            if (shouldMove)
            {
                rb.AddForce(targetDirection * moveSpeed);
            }
        }
        
        private bool HandleWandering(out Vector2 direction)
        {
            direction = Vector2.zero;
            wanderTimer += Time.fixedDeltaTime;

            float cycleTime = wanderTimer % 5f;
        
            if (cycleTime < 2f)
            {
                if (!isWandering)
                {
                    isWandering = true;
                    currentWanderDirection = UnityEngine.Random.insideUnitCircle.normalized;
                }
                direction = currentWanderDirection;
                return true;
            }
        
            isWandering = false;
            return false;
        }

        public Vector3 GetTargetPosition()
        {
            return transform.position;
        }
        
        public void AddTargetingRule(Predicate<AIView> predicate)
        {
            targetingRules.Add(predicate);
        }

        public AIView GetView()
        {
            return new AIView()
            {
                CellSize = CellSize,
                MatingEnum = matingEnum,
                Deviation = DeviationEnum.Red
            };
        }
        
        private bool isInitializedByFactory;
        public void Initialize(MatingEnum matingEnum, CellSize cellSize, Vector3 position)
        {
            this.matingEnum = matingEnum;
            this.cellSize = cellSize;
            transform.position = position;
            isInitializedByFactory = true;
            
            SetupTargetingRules();
            
            OnReinitialized?.Invoke(this);
        }

        private void SetupTargetingRules()
        {
            targetingRules.Clear();
            targetingRules.Add(view => view.CellSize == cellSize && view.Deviation == DeviationEnum.Default);
        }

        private void Start()
        {
            if (!isInitializedByFactory)
            {
                SetupTargetingRules();
            }
            navigationSystem.RegisterTarget(this);
        }
        
        public class Factory : PlaceholderFactory<RedCell>
        {
            
        }
    }
}