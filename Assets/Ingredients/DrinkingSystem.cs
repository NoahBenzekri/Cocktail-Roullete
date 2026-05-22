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

    public System.Action OnClockExpired;


    public void Update()
    {
        if (isFrozen || !IsAlive) return;

        if (_isReverse)
        {
            clockTime += Time.deltaTime;
        }
        else
        {
            clockTime -= Time.deltaTime * drainMultiplier;
        }

        if (clockTime <= 0f)
        {
            Debug.Log("Clock expired you have no time left!");
            clockTime = 0f;
            OnClockExpired?.Invoke();
        }
    }
    public void Drink()
    {
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

        List<IngredientsOBJ> resolvedIngredients = GetResolvedIngredients(cocktailGlass.ingredientInGlass);

        foreach (IngredientsOBJ ingredient in resolvedIngredients)
        {
            ApplyEffect(ingredient);
        }

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
            {
                resolved.Add(ingredient);
            }
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
                clockTime -= amount;
                Debug.Log("Venom poison damage: " + amount);
                break;

            case DrinkEffectType.FrostBite:
                StartCoroutine(FreezeRoutine(amount));
                Debug.Log("Frostbite freeze: " + amount);
                break;

            case DrinkEffectType.Acid:
                StartCoroutine(AcidRoutine(amount));
                // call acid effect
                Debug.Log("Acid drain multiplier for: " + amount);
                break;

            case DrinkEffectType.LiquidLuck:
                // call liquid luck effect
                StartCoroutine(ReverseRoutine());
                Debug.Log("Liquid Luck activated. AI should become dumber later.");
                break;

            case DrinkEffectType.Blackout:
                clockTime = 0f;
                Debug.Log("Blackout. Instant death.");
                OnClockExpired?.Invoke();
                break;
        }

        clockTime = Mathf.Max(clockTime, 0f);
    }
    public IEnumerator FreezeRoutine(float duration)
    {
        isFrozen = true;
        Debug.Log("Player is frozen!");
        yield return new WaitForSeconds(duration); // Freeze duration demo gonna have it linked to game manager to stop on next round or something 
        isFrozen = false;
        Debug.Log("Clock is unfrozen!");
    }

    public IEnumerator AcidRoutine(float duration)
    {
        drainMultiplier = 2f;
        Debug.Log("Timer drain doubled");

        yield return new WaitForSeconds(duration);

        drainMultiplier = 1f;
        Debug.Log("Timer drain back to normal");
    }
    public IEnumerator ReverseRoutine()
    {
        _isReverse = true;
        Debug.Log("Clock is reversed!");
        yield return new WaitForSeconds(5f); // Reverse duration Demo
        _isReverse = false;
        Debug.Log("Clock is back to normal!");
    }

    public IEnumerator UltimatePenaltyRoutine()
    {
        clockTime = 0f;
        Debug.Log("Ultimate penalty applied! Clock is now at 0!");
        yield return null;
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