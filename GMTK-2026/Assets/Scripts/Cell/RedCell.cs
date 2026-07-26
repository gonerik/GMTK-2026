using System;
using System.Collections.Generic;
using Cell;
using Cell.Visual;
using Cysharp.Threading.Tasks;
using DefaultNamespace.Zenject;
using Energy;
using Interfaces;
using MateStrategy;
using UnityEngine;
using Zenject;

namespace DefaultNamespace
{
    public class RedCell : MonoBehaviour, IVisualyConfigurable, ITarget, IEntity, ISelectable
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
        [Inject] private CellLifetimeConfig lifetimeConfig;
        private List<Predicate<AIView>> targetingRules = new List<Predicate<AIView>>();
        private Rigidbody2D rb;
        private float wanderTimer;
        private bool isWandering;
        private Vector2 currentWanderDirection;
        private float age;
        private float ageRate = 1;
        
        private const string DieSound = "event:/Cell dies";
        

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            LifeTimeTask().Forget();
        }
        
        private async UniTaskVoid LifeTimeTask()
        {
            age = lifetimeConfig.CalculateLifetime(matingEnum, cellSize, Deviation);
            while (age >= 0)
            {
                age -= ageRate;
                await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: this.GetCancellationTokenOnDestroy());
            }
            FMODUnity.RuntimeManager.PlayOneShot(DieSound);
            Destroy();
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
            foreach (var rule in targetingRules)
            {
                if (rule.Invoke(target.GetView()))
                {
                    return true;
                }
            }
            return false;
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.TryGetComponent(out CellUnit cell))
            {
                if (cell.CellSize == CellSize && !cell.IsMating)
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

        public SelectionInfo GetSelectionInfo()
        {
            return new SelectionInfo()
            {
                CellSize = CellSize,
                Deviation = Deviation,
                MatingEnum = matingEnum,
                EnergyAmount = energyAmount,
                speed = moveSpeed,
                Age = age,
                MaxAge = lifetimeConfig.CalculateLifetime(matingEnum, cellSize, Deviation)
            };
        }

        public void SetAgeRate(float ageRate)
        {
            this.ageRate = ageRate;
            Debug.Log("Age rate set to " + ageRate);
        }
        
        private bool isInitializedByFactory;
        public void Initialize(MatingEnum matingEnum, CellSize cellSize, Vector3 position)
        {
            this.matingEnum = matingEnum;
            this.cellSize = cellSize;
            transform.position = position;
            isInitializedByFactory = true;
            
            SetupTargetingRules();
            
            visualAssembler.Reassemble(this);
            OnReinitialized?.Invoke(this);
        }

        private void SetupTargetingRules()
        {
            targetingRules.Clear();
            targetingRules.Add(view => view.CellSize == CellSize && view.Deviation != DeviationEnum.Red);
        }

        private void Start()
        {
            if (!isInitializedByFactory)
            {
                SetupTargetingRules();
                visualAssembler.Reassemble(this);
            }
            navigationSystem.RegisterTarget(this);
        }
        
        public class Factory : PlaceholderFactory<RedCell>
        {
            
        }
    }
}