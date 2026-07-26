using System;
using System.Collections.Generic;
using Cell;
using Cell.Visual;
using Cysharp.Threading.Tasks;
using DefaultNamespace;
using DefaultNamespace.Zenject;
using DG.Tweening;
using Energy;
using FMODUnity;
using Interfaces;
using MateStrategy;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

public class CellUnit : MonoBehaviour, IMate, IVisualyConfigurable, ISelectable
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private CellSize cellSize;
    [SerializeField] private CellColor colorType;
    [SerializeField] private CellMembrane membraneType;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private GrowStrategyEnum growStrategy;
    [SerializeField] private MatingEnum matingEnum;
    private IColor cellColor;
    private IMembrane cellMembrane;
    private ISize sizeStrategy;
    private Vector2 currentWanderDirection;
    private IDeviation deviationStrategy;
    private float ageRate;
    private float age;
    [Inject] private EnergyService energyService;

    private bool isInitializedByFactory;
    private bool isWandering;
    private SpriteRenderer spriteRenderer;
    [Inject] private CellLifetimeConfig lifetimeConfig;
    [Inject] private MatingService matingService;
    [Inject] private Acid.Factory acidFactory;
    private IMateStrategy matiStrategy;

    private string appearSound;
    private const string DieSound = "event:/Cell dies";
    
    

    [Inject] private NavigationSystem navigationSystem;

    private readonly List<Predicate<AIView>> targetingRules = new();
    [Inject] private CellVisualAssembler visualAssembler;

    private float wanderTimer;

    private void Start()
    {
        Initialize(matingEnum, cellSize, DeviationEnum.Default, transform.position);
        navigationSystem.RegisterTarget(this);
        LifeTimeTask().Forget();
        FMODUnity.RuntimeManager.PlayOneShot(appearSound);
    }

    private void FixedUpdate()
    {
        var targetDirection = Vector2.zero;
        var shouldMove = false;

        if (navigationSystem.targets.TryGetValue(this, out var target))
        {
            var toTarget = (Vector2)target.GetTargetPosition() - (Vector2)transform.position;
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

        if (shouldMove) rb.AddForce(targetDirection * moveSpeed);
    }

    private void OnDestroy()
    {
        OnDie?.Invoke(this);
        UnsubscribeFromStrategies();
        energyService.AddEnergy(EnergyAmount);
        navigationSystem.UnregisterTarget(this);
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
        
        if (spriteRenderer != null)
        {
            Vector3 startLocalScale = spriteRenderer.transform.localScale;
            spriteRenderer.transform.DOScale(startLocalScale * 0.9f, 0.1f).SetEase(Ease.InCubic).OnComplete(() => spriteRenderer.transform.DOScale(startLocalScale, 0.1f).SetEase(Ease.InBack));
        }
    }

    public float DetectionRange => detectionRange;
    public CellSize CellSize => cellSize;
    public int EnergyAmount => lifetimeConfig.CalculateEnergyGain(matingEnum, cellSize, Deviation);

    public bool IsMating { get; set; }

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
        foreach (var rule in targetingRules)
        {
            if (rule.Invoke(target.GetView()))
            {
                return true;
            }
        }
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
        return new AIView
        {
            CellSize = this.CellSize,
            MatingEnum = GetMatingEnum(),
            Deviation = this.Deviation
        };
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }

    public MatingEnum MatingEnum => matingEnum;
    public DeviationEnum Deviation { get; private set; }

    public Transform GetTransform()
    {
        return transform;
    }

    public event Action<IVisualyConfigurable> OnReinitialized;

    public event Action<IMate> OnMate;

    public event Action<CellUnit> OnDie;

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
    
    public void SetSpawnSound(string id)
    {
        appearSound = id;
    }

    public void Initialize(MatingEnum matingEnum, CellSize cellSize, DeviationEnum deviationEnum, Vector3 position)
    {
        this.matingEnum = matingEnum;
        this.cellSize = cellSize;
        this.Deviation = deviationEnum;
        if(deviationEnum == DeviationEnum.Red)
        {
            Deviation = DeviationEnum.Default;
        }

        if (deviationEnum == DeviationEnum.Blue)
        {
            moveSpeed = 5f;
        }
        
        transform.position = position;
        isInitializedByFactory = true;
        InitializeStrategies();
    }

    private void InitializeStrategies()
    {
        targetingRules.Clear();
        UnsubscribeFromStrategies();

        sizeStrategy = CellStrategyFactory.CreateSize(cellSize, acidFactory);
        matiStrategy = CellStrategyFactory.CreateMateStrategy(matingEnum);
        cellColor = CellStrategyFactory.CreateColor(colorType);
        cellMembrane = CellStrategyFactory.CreateMembrane(membraneType);
        

        cellColor.Initialize(this);
        cellMembrane.Initialize(this);
        matiStrategy.Initialize(this);
        sizeStrategy.Initialize(this);
        spriteRenderer = visualAssembler.Reassemble(this);

        OnReinitialized?.Invoke(this);
    }
    
    public void SetAgeRate(float ageRate)
    {
        this.ageRate = ageRate;
        Debug.Log("Age rate set to " + ageRate);
    }

    public void SetVisionRange(float visionRange)
    {
        detectionRange = visionRange;
    }
    
    private void UnsubscribeFromStrategies()
    {
        cellColor?.Unsubscribe(this);
        cellMembrane?.Unsubscribe(this);
        matiStrategy?.Unsubscribe(this);
        sizeStrategy?.Unsubscribe(this);
    }

    private bool HandleWandering(out Vector2 direction)
    {
        direction = Vector2.zero;
        wanderTimer += Time.fixedDeltaTime;

        var cycleTime = wanderTimer % 5f;

        if (cycleTime < 2f)
        {
            if (!isWandering)
            {
                isWandering = true;
                currentWanderDirection = Random.insideUnitCircle.normalized;
            }

            direction = currentWanderDirection;
            return true;
        }

        isWandering = false;
        return false;
    }

    public class Factory : PlaceholderFactory<CellUnit>
    {
    }

    public SelectionInfo GetSelectionInfo()
    {
        return new SelectionInfo()
        {
            CellSize = cellSize,
            Deviation = Deviation,
            MatingEnum = matingEnum,
            EnergyAmount = EnergyAmount,
            speed = moveSpeed,
            Age = age,
            MaxAge = lifetimeConfig.CalculateLifetime(matingEnum, cellSize, Deviation)
        };
    }
}