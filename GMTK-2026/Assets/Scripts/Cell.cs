using System;
using System.Collections;
using System.Collections.Generic;
using Interfaces;
using UnityEngine;
using DefaultNamespace;

public class Cell : MonoBehaviour, IEatable, IConsumer
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private CellSize cellSize;
    [SerializeField] private CellColor colorType;
    [SerializeField] private CellMembrane membraneType;
    [SerializeField] private float speed = 10f;
    [SerializeField] private float visionRadius = 1f;

    private List<Predicate<(IConsumer, IEatable)>> eatRules = new List<Predicate<(IConsumer, IEatable)>>();
    private IColor cellColor;
    private IMembrane cellMembrane;

    void Start()
    {
        InitializeStrategies();
        AddEatRule(new Predicate<(IConsumer, IEatable)>((x) => x.Item1.CellSize - 1 == x.Item2.CellSize));
    }

    private void FixedUpdate()
    {
        MoveTowardsClosestEatable();
    }

    private void MoveTowardsClosestEatable()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, visionRadius);
        IEatable closestEatable = null;
        float minDistance = float.MaxValue;

        foreach (var collider in colliders)
        {
            if (collider.gameObject == gameObject) continue;

            IEatable eatable = collider.GetComponent<IEatable>();
            if (eatable != null && eatable.CanBeEaten(this))
            {
                float distance = Vector2.Distance(transform.position, collider.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestEatable = eatable;
                }
            }
        }

        if (closestEatable != null && closestEatable is MonoBehaviour eatableMono)
        {
            Vector2 direction = ((Vector2)eatableMono.transform.position - (Vector2)transform.position).normalized;
            rb.AddForce(direction * speed);
        }
    }

    public void InitializeStrategies()
    {
        cellColor = CellStrategyFactory.CreateColor(colorType);
        cellMembrane = CellStrategyFactory.CreateMembrane(membraneType);

        cellColor.Initialize(this);
        cellMembrane.Initialize(this);
    }

    void Update()
    {

    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.TryGetComponent(out IEatable eatable))
        {
            Consume(eatable);
        }
    }

    public CellSize CellSize => cellSize;

    public virtual void Consume(IEatable eatable)
    {
        if (!eatable.CanBeEaten(this))
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
