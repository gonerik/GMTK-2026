using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using Interfaces;
using UnityEngine;

public class Cell : MonoBehaviour, IEatable, IConsumer
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Size size;
    
    private List<Predicate<(IConsumer, IEatable)>> eatRules = new List<Predicate<(IConsumer, IEatable)>>();
    
    void Start()
    {
        AddEatRule(new Predicate<(IConsumer, IEatable)>((x) => x.Item1.Size < x.Item2.Size));
    }
    void Update()
    {
        
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
