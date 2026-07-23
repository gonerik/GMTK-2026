using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DefaultNamespace;
using DefaultNamespace.Zenject;
using Interfaces;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Zenject;

public class Cell : MonoBehaviour, IEatable, IConsumer
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Size size;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float moveSpeed = 5f;
    
    private List<Predicate<(IConsumer, IEatable)>> eatRules = new List<Predicate<(IConsumer, IEatable)>>();
    
    private float wanderTimer;
    private Vector2 currentWanderDirection;
    private bool isWandering;
    
    [Inject] private NavigationSystem navigationSystem;

    void Start()
    {
        navigationSystem.RegisterConsumer(this);
        navigationSystem.RegisterEatable(this);
        AddEatRule(new Predicate<(IConsumer, IEatable)>((x) => x.Item1.Size - 1 == x.Item2.Size));
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

    public Size Size => size;
    public float DetectionRange => detectionRange;
    
    public virtual void Consume(IEatable eatable)
    {
        if(!eatable.CanBeEaten(this))
        {
            return;
        }
        eatable.Eat(this);
    }

    public void Grow()
    {
        
    }

    public virtual void Eat(IConsumer consumer)
    {
        gameObject.SetActive(false);
    }

    public void AddEatRule(Predicate<(IConsumer, IEatable)> rule)
    {
        eatRules.Add(rule);
    }

    public bool CanBeEaten(IConsumer consumer)
    {
        bool canBeEaten = true;
        foreach (var rule in eatRules)
        {
            canBeEaten = rule.Invoke((consumer, this)) && canBeEaten;
        }
        return canBeEaten;
    }
}
