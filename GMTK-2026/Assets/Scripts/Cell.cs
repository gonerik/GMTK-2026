using System;
using System.Collections.Generic;
using DefaultNamespace;
using DefaultNamespace.GrowStrategy;
using DefaultNamespace.Zenject;
using Interfaces;
using UnityEngine;
using Zenject;

public class Cell : MonoBehaviour, IEatable, IConsumer
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private CellSize cellSize;
    [SerializeField] private CellColor colorType;
    [SerializeField] private CellMembrane membraneType;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private GrowStrategyEnum growStrategy;
    public float DetectionRange => detectionRange;
    public CellSize CellSize => cellSize;
    public int GrowThreshold { get; set; } = 1;

    private List<Predicate<IEatable>> eatRules = new List<Predicate<IEatable>>();
    private IColor cellColor;
    private IMembrane cellMembrane;
    private List<IEatable> eaten = new List<IEatable>();
    //#TODO Add enum for GrowStrategy
    private Dictionary<Type, int> growStrategies = new Dictionary<Type, int>();

    public event Action<IEatable> OnConsume;
    public event Action<IConsumer> OnEat;
    public event Action<IConsumer> OnGrow;

    
    private float wanderTimer;
    private Vector2 currentWanderDirection;
    private bool isWandering;
    
    
    [Inject] private NavigationSystem navigationSystem;

    void Start()
    {
        InitializeStrategies();
        AddEatRule(new Predicate<IEatable>(x => CellSize - 1  == x.CellSize));
        navigationSystem.RegisterConsumer(this);
        navigationSystem.RegisterEatable(this);
    }

    private void OnDestroy()
    {
        navigationSystem.UnregisterConsumer(this);
        navigationSystem.UnregisterEatable(this);
    }

    void FixedUpdate()
    {
        Vector2 targetDirection = Vector2.zero;
        bool shouldMove = false;

        if (navigationSystem.targets.TryGetValue(this, out var target))
        {
            if (target is MonoBehaviour targetMb && targetMb != null)
            {
                Vector2 toTarget = (Vector2)targetMb.transform.position - (Vector2)transform.position;
                targetDirection = toTarget.normalized;
                shouldMove = true;
            }
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

    private void InitializeStrategies()
    {
        cellColor?.Unsubscribe(this);
        cellMembrane?.Unsubscribe(this);

        cellColor = CellStrategyFactory.CreateColor(colorType);
        cellMembrane = CellStrategyFactory.CreateMembrane(membraneType);

        cellColor.Initialize(this);
        cellMembrane.Initialize(this);
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
        if (other.gameObject.TryGetComponent(out IEatable eatable))
        {
            Consume(eatable);
        }
    }

    public virtual void Consume(IEatable eatable)
    {
        if (!CanBeEaten(eatable))
        {
            return;
        }

        OnConsume?.Invoke(eatable);
        eaten.Add(eatable);
        IGrowStrategy strategy = eatable.Eat(this);
        if (!growStrategies.ContainsKey(strategy.GetType()))
        {
            growStrategies.Add(strategy.GetType(), 0);
        }
        growStrategies[strategy.GetType()]++;
        TryGrow();
    }

    public void TryGrow()
    {
        if (GrowThreshold <= eaten.Count)
        {
            OnGrow?.Invoke(this);
            InitializeStrategies();
        }
    }

    public virtual IGrowStrategy Eat(IConsumer consumer)
    {
        OnEat?.Invoke(consumer);
        return CellStrategyFactory.CreateGrowStrategy(growStrategy);
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }

    public void AddEatRule(Predicate<IEatable> rule)
    {
        eatRules.Add(rule);
    }

    public bool CanBeEaten(IEatable eatable)
    {
        bool canBeEaten = true;
        foreach (var rule in eatRules)
        {
            canBeEaten = rule.Invoke(eatable) && canBeEaten;
        }
        return canBeEaten;
    }
}
