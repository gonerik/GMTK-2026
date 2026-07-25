using System;
using System.Collections.Generic;
using Cell.Visual;
using Cysharp.Threading.Tasks;
using DefaultNamespace;
using DefaultNamespace.Zenject;
using Energy;
using Interfaces;
using MateStrategy;
using UnityEngine;
using Zenject;

    public class CellUnit : MonoBehaviour, IMate, IVisualyConfigurable
    {
        public class Factory : PlaceholderFactory< CellUnit>
        {
        } 
        
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private CellSize cellSize;
        [SerializeField] private CellColor colorType;
        [SerializeField] private CellMembrane membraneType;
        [SerializeField] private float detectionRange = 10f;
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private GrowStrategyEnum growStrategy;
        [SerializeField] private MatingEnum matingEnum;
        [SerializeField] private int energyAmount;
    public float DetectionRange => detectionRange;
    public CellSize CellSize => cellSize;
    public MatingEnum MatingEnum => matingEnum;
    public int EnergyAmount => energyAmount;
    public DeviationEnum Deviation => DeviationEnum.Default;
    public Transform GetTransform()
    {
        return transform;
    }

    private List<Predicate<AIView>> targetingRules = new List<Predicate<AIView>>();
    private IColor cellColor;
    private IMembrane cellMembrane;
    private IMateStrategy matiStrategy;
    private IDeviation deviationStrategy;
    
    public bool IsMating { get; set; }

    public event Action<IMate> OnMate;
    public event Action<IVisualyConfigurable> OnReinitialized;
    
    private float wanderTimer;
    private Vector2 currentWanderDirection;
    private bool isWandering;
    
    [Inject] private NavigationSystem navigationSystem;
    [Inject] private MatingService matingService;
    [Inject] private EnergyService energyService;
    [Inject] private CellVisualAssembler visualAssembler;

    void Start()
    {
        InitializeStrategies();
        navigationSystem.RegisterTarget(this);
    }

    private void OnDestroy()
    {
        UnsubscribeFromStrategies();
        energyService.AddEnergy(energyAmount);
        navigationSystem.UnregisterTarget(this);
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
    
    private bool isInitializedByFactory;
    public void Initialize(MatingEnum matingEnum, CellSize cellSize, Vector3 position)
    {
        this.matingEnum = matingEnum;
        this.cellSize = cellSize;
        transform.position = position;
        isInitializedByFactory = true;
        InitializeStrategies();
    }

    private void InitializeStrategies()
    {
        targetingRules.Clear();
        UnsubscribeFromStrategies();

        matiStrategy = CellStrategyFactory.CreateMateStrategy(matingEnum);
        cellColor = CellStrategyFactory.CreateColor(colorType);
        cellMembrane = CellStrategyFactory.CreateMembrane(membraneType);
        
        cellColor.Initialize(this);
        cellMembrane.Initialize(this);
        matiStrategy.Initialize(this);
        visualAssembler.Reassemble(this);
        
        OnReinitialized?.Invoke(this);
    }

    private void UnsubscribeFromStrategies()
    {
        cellColor?.Unsubscribe(this);
        cellMembrane?.Unsubscribe(this);
        matiStrategy?.Unsubscribe(this);
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

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.TryGetComponent(out IMate partner))
        {
            if (!IsMating && !partner.IsMating)
            {
                Mate(partner);
            }
        }
    }

    public virtual void Mate(IMate partner)
    {
        matingService.Mate(this, partner);
    }

    public MatingEnum GetMatingEnum()
    {
        return matingEnum;
    }

    public bool CanTarget(ITarget target)
    {
        bool shouldTarget = true;
        foreach (var rule in targetingRules)
        {
            shouldTarget = rule.Invoke(target.GetView()) && shouldTarget;
        }
        return shouldTarget;
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
            MatingEnum = GetMatingEnum(),
            Deviation = DeviationEnum.Default
        };
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }
    
    }
