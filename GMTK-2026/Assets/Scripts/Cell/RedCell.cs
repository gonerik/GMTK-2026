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
        [SerializeField, Min(0), Tooltip("Energy lost each time this Predator's Bite kills a cell, on top of the victim's own payout.")]
        private int killPenalty = 5;
        [SerializeField, Min(0.05f), Tooltip("Seconds between this Predator's Bites while it touches cells. The first Bite lands on touch.")]
        private float biteInterval = 1f;
        [SerializeField, Min(1), Tooltip("Bites a Small Predator needs to kill a fresh Small cell. A cell takes this many per stage, and a Predator bites for its own stage.")]
        private int bitesPerStage = 3;

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
        private SpriteRenderer spriteRenderer;
        // Cells this Predator is touching, with the collider each touched it through.
        private readonly Dictionary<CellUnit, Collider2D> contacts = new Dictionary<CellUnit, Collider2D>();
        private readonly List<CellUnit> biteBuffer = new List<CellUnit>();
        private float nextBiteTime;

        private const string DieSound = "event:/Cell dies";
        private const float LifeTick = 1f;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }
        
        // Share of its lifespan this Predator has left, 1..0. Drives its opacity.
        private float RemainingLife
        {
            get
            {
                int lifetime = lifetimeConfig.CalculateLifetime(matingEnum, cellSize, Deviation);
                return lifetime > 0 ? age / lifetime : 0f;
            }
        }

        private async UniTaskVoid LifeTimeTask()
        {
            age = lifetimeConfig.CalculateLifetime(matingEnum, cellSize, Deviation);
            while (age >= 0)
            {
                age -= ageRate;
                visualAssembler.ShowRemainingLife(spriteRenderer, RemainingLife, LifeTick);
                await UniTask.Delay(TimeSpan.FromSeconds(LifeTick), cancellationToken: this.GetCancellationTokenOnDestroy());
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
            energyService.AddEnergy(energyAmount, transform.position);
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
            // A Predator Bites any cell it touches, whatever its stage or strain. Acid and other Predators
            // are not CellUnits, so they are never touched.
            if (other.gameObject.TryGetComponent(out CellUnit cell))
            {
                contacts[cell] = other.collider;
                TryBite();
            }
        }

        private void OnCollisionExit2D(Collision2D other)
        {
            if (other.gameObject.TryGetComponent(out CellUnit cell))
            {
                contacts.Remove(cell);
            }
        }

        // Bites every cell still touching, at most once per biteInterval. The first Bite lands on touch, and
        // the one cooldown means bouncing on and off a cell never bites faster.
        private void TryBite()
        {
            if (contacts.Count == 0 || Time.time < nextBiteTime) return;

            biteBuffer.Clear();
            biteBuffer.AddRange(contacts.Keys);
            bool bitten = false;
            foreach (var cell in biteBuffer)
            {
                // Exit callbacks are not guaranteed when a cell is destroyed or its collider is disabled by a
                // drag, so check the contact is still live. A cell latched IsMating is already being destroyed.
                if (!cell || cell.IsMating || !rb.IsTouching(contacts[cell]))
                {
                    contacts.Remove(cell);
                    continue;
                }
                Bite(cell);
                bitten = true;
            }

            if (bitten)
            {
                nextBiteTime = Time.time + biteInterval;
            }
        }

        // A cell takes bitesPerStage Bites' worth per stage, and a Predator bites for its own stage. Only the
        // killing Bite costs killPenalty energy; the victim still dies paying its own payout.
        private void Bite(CellUnit cell)
        {
            float share = StageRank(cellSize) / (float)(bitesPerStage * StageRank(cell.CellSize));
            if (cell.TakeBite(share))
            {
                energyService.AddEnergy(-killPenalty, cell.transform.position);
                contacts.Remove(cell);
            }
        }

        private static int StageRank(CellSize stage)
        {
            switch (stage)
            {
                case CellSize.Medium:
                    return 2;
                case CellSize.Large:
                    return 3;
                default:
                    return 1;
            }
        }

        void FixedUpdate()
        {
            TryBite();

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

            spriteRenderer = visualAssembler.Reassemble(this);
            OnReinitialized?.Invoke(this);
        }

        private void SetupTargetingRules()
        {
            targetingRules.Clear();
            // Hunts every cell of any stage or strain, except Acid and other Predators.
            targetingRules.Add(view => view.CellSize != CellSize.Acid && view.Deviation != DeviationEnum.Red);
        }

        private void Start()
        {
            if (!isInitializedByFactory)
            {
                SetupTargetingRules();
                spriteRenderer = visualAssembler.Reassemble(this);
            }
            navigationSystem.RegisterTarget(this);
            LifeTimeTask().Forget();
        }
        
        public class Factory : PlaceholderFactory<RedCell>
        {
            
        }
    }
}