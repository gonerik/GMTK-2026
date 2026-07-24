using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DefaultNamespace;
using DefaultNamespace.Zenject;
using Interfaces;
using MateStrategy;
using UnityEngine;
using Zenject;

    public class Cell : MonoBehaviour, IMate
    {
        public class Factory : PlaceholderFactory< Cell>
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
    public float DetectionRange => detectionRange;
    public CellSize CellSize => cellSize;

    private List<Predicate<AIView>> targetingRules = new List<Predicate<AIView>>();
    private IColor cellColor;
    private IMembrane cellMembrane;
    private IMateStrategy matiStrategy;
    
    public bool IsMating { get; set; }

    public event Action<IMate> OnMate;
    
    private float wanderTimer;
    private Vector2 currentWanderDirection;
    private bool isWandering;
    
    [Inject] private NavigationSystem navigationSystem;
    [Inject] private MatingService matingService;

    void Start()
    {
        InitializeStrategies();
        navigationSystem.RegisterTarget(this);
    }

    private void OnDestroy()
    {
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
    
    public void Initialize(MatingEnum matingEnum, CellSize cellSize, Vector3 position)
    {
        this.matingEnum = matingEnum;
        this.cellSize = cellSize;
        transform.position = position;
        InitializeStrategies();
    }

    private void InitializeStrategies()
    {
        targetingRules.Clear();
        cellColor?.Unsubscribe(this);
        cellMembrane?.Unsubscribe(this);

        matiStrategy = CellStrategyFactory.CreateMateStrategy(matingEnum);
        cellColor = CellStrategyFactory.CreateColor(colorType);
        cellMembrane = CellStrategyFactory.CreateMembrane(membraneType);
        
        cellColor.Initialize(this);
        cellMembrane.Initialize(this);
        matiStrategy.Initialize(this);
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
            MatingEnum = GetMatingEnum()
        };
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }
}
