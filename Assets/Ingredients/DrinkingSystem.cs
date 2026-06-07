using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class DrinkingSystem : MonoBehaviour
{
    public CocktailGlass cocktailGlass;

    public float clockTime = 90f;
    public bool IsAlive => clockTime > 0f;

    public bool isFrozen = false;
    public bool _isReverse = false;

    public float drainMultiplier = 1f;
    public bool catalystActive = false;

    private int venomRoundsLeft = 0;

    public System.Action OnClockExpired;

    public void Update()
    {
        if (clockTime <= 0f)
        {
            clockTime = 0f;
            OnClockExpired?.Invoke();
            return;
        }

        if (isFrozen || !IsAlive) return;

        if (_isReverse)
            clockTime += Time.deltaTime;
        else
            clockTime -= Time.deltaTime * drainMultiplier;

        if (clockTime <= 0f)
        {
            clockTime = 0f;
            OnClockExpired?.Invoke();
        }
    }

    public void Drink()
    {
        Debug.Log("Drink() called on: " + gameObject.name);

        if (cocktailGlass == null)
        {
            Debug.LogError("No cocktail glass assigned to DrinkingSystem!");
            return;
        }

        if (!cocktailGlass.HasIngredients())
        {
            Debug.Log("Glass is empty!");
            return;
        }

        // tick venom before resolving new effects
        if (venomRoundsLeft > 0)
        {
            venomRoundsLeft--;
            float venomDamage = Random.value < 0.6f ? 10f : 0f; // 60% chance to hit
            if (venomDamage > 0f)
            {
                clockTime -= venomDamage;
                clockTime = Mathf.Max(clockTime, 0f);
                Debug.Log("Venom ticked! -10 seconds. Rounds left: " + venomRoundsLeft);
            }
            else
            {
                Debug.Log("Venom missed this round. Rounds left: " + venomRoundsLeft);
            }
        }

        List<IngredientsOBJ> resolvedIngredients = GetResolvedIngredients(cocktailGlass.ingredientInGlass);

        foreach (IngredientsOBJ ingredient in resolvedIngredients)
            ApplyEffect(ingredient);

        cocktailGlass.ClearGlass();
    }

    private List<IngredientsOBJ> GetResolvedIngredients(List<IngredientsOBJ> ingredients)
    {
        List<IngredientsOBJ> resolved = new List<IngredientsOBJ>();

        foreach (IngredientsOBJ ingredient in ingredients)
        {
            bool isCountered = false;
            foreach (IngredientsOBJ other in ingredients)
            {
                if (ingredient == other) continue;
                if (other.effectType == ingredient.CounterEffectType)
                {
                    isCountered = true;
                    Debug.Log("Ingredient " + ingredient.name + " is countered by " + other.name + " and has no effect!");
                    break;
                }
            }
            if (!isCountered)
                resolved.Add(ingredient);
        }

        return resolved;
    }

    private void ApplyEffect(IngredientsOBJ ingredient)
    {
        float amount = ModifyAmount(ingredient.amount);

        switch (ingredient.effectType)
        {
            case DrinkEffectType.Catalyst:
                catalystActive = true;
                Debug.Log("Catalyst activated. Next effect doubled.");
                break;

            case DrinkEffectType.Venom:
                // apply first tick immediately with 60% chance, then linger for 2 rounds
                venomRoundsLeft = 2;
                float firstTick = Random.value < 0.6f ? 10f : 0f;
                if (firstTick > 0f)
                {
                    clockTime -= firstTick;
                    clockTime = Mathf.Max(clockTime, 0f);
                    Debug.Log("Venom first tick hit! -10 seconds.");
                }
                else
                {
                    Debug.Log("Venom first tick missed.");
                }
                break;

            case DrinkEffectType.FrostBite:
                // slow timer to half speed for `amount` seconds
                StartCoroutine(FrostBiteRoutine(amount));
                Debug.Log("Frostbite: timer slowed for " + amount + " seconds.");
                break;

            case DrinkEffectType.Acid:
                // drain twice as fast for `amount` seconds
                StartCoroutine(AcidRoutine(amount));
                Debug.Log("Acid: drain doubled for " + amount + " seconds.");
                break;

            case DrinkEffectType.LiquidLuck:
                // extra life
                clockTime += 90f;
                Debug.Log("Liquid Luck: extra life granted! Clock reset +90s.");
                break;

            case DrinkEffectType.Blackout:
                Debug.Log("BLACKOUT on: " + gameObject.name);
                clockTime = 0f;
                OnClockExpired?.Invoke();
                break;
        }

        Debug.Log("Applied: " + ingredient.effectType + " | Amount: " + amount + " | On: " + gameObject.name);
        clockTime = Mathf.Max(clockTime, 0f);
    }

    public IEnumerator FrostBiteRoutine(float duration)
    {
        drainMultiplier = 0.5f; // half speed
        Debug.Log("FrostBite: timer slowed.");
        yield return new WaitForSeconds(duration);
        drainMultiplier = 1f;
        Debug.Log("FrostBite: timer back to normal.");
    }

    public IEnumerator AcidRoutine(float duration)
    {
        drainMultiplier = 2f; // twice as fast
        Debug.Log("Acid: drain doubled.");
        yield return new WaitForSeconds(duration);
        drainMultiplier = 1f;
        Debug.Log("Acid: drain back to normal.");
    }

    public IEnumerator ReverseRoutine()
    {
        _isReverse = true;
        Debug.Log("Clock reversed!");
        yield return new WaitForSeconds(5f);
        _isReverse = false;
        Debug.Log("Clock back to normal.");
    }

    private float ModifyAmount(float amount)
    {
        if (catalystActive)
        {
            catalystActive = false;
            return amount * 2f;
        }
        return amount;
    }
}