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
    [SerializeField] private float moveForce = 5f;

    private List<Predicate<(IConsumer, IEatable)>> eatRules = new List<Predicate<(IConsumer, IEatable)>>();
    private Vector2 moveDirection;
    
    [Inject] private NavigationSystem navigationSystem;

    void Start()
    {
        AddEatRule(new Predicate<(IConsumer, IEatable)>((x) => x.Item1.Size < x.Item2.Size));
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
        if (navigationSystem.targets.TryGetValue(this, out var target))
        {
            if (target is MonoBehaviour targetMb && targetMb != null)
            {
                Vector2 direction = (targetMb.transform.position - transform.position).normalized;
                rb.AddForce(direction * moveForce);
            }
        }
    }

    public Size Size => size;
    
    public virtual void Consume(IEatable eatable)
    {
        if(!eatable.CanBeEaten(this))
        {
            return;
        }
        eatable.Eat(this);
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
