using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class DrinkingSystem : MonoBehaviour
{

    public CocktailGlass cocktailGlass;

    public float clockTime = 60f;
    public bool IsAlive => clockTime > 0f;

    public bool isFrozen = false;
    public bool _isReverse = false;

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
            clockTime -= Time.deltaTime;
        }


        if (clockTime <= 0f)
        {
            Debug.Log("Clock expired you have no time left!");
            clockTime = 0f;
            OnClockExpired?.Invoke();
        }
    }
    public IEnumerator FreezeRoutine()
    {
        isFrozen = true;
        Debug.Log("Player is frozen!");
        yield return new WaitForSeconds(10f); // Freeze duration demo gonna have it linked to game manager to stop on next round or something 
        Debug.Log("Clock is unfrozen!");
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

        foreach (IngredientsOBJ ingredient in cocktailGlass.ingredientInGlass)
        {
            ApplyEffect(ingredient);
        }

        cocktailGlass.ClearGlass();
    }

    private void ApplyEffect(IngredientsOBJ ingredient)
    {
        switch (ingredient.ingredientType)
        {
            case IngredientType.Positive:
                clockTime += 10f; //DEMO
                Debug.Log("Positive effect: " + ingredient.name);
                break;

            case IngredientType.Negative:
                clockTime -= 10f; // DEMO
                Debug.Log("Negative effect: " + ingredient.name);
                break;

            case IngredientType.Illusion:
                StartCoroutine(FreezeRoutine());
                Debug.Log("Illusion effect: " + ingredient.name);
                break;

            case IngredientType.Special:
                clockTime -= 5f; // DEMO
                Debug.Log("Special effect: " + ingredient.name);
                break;

            case IngredientType.Reverse:
                StartCoroutine(ReverseRoutine());
                Debug.Log("Reverse effect: " + ingredient.name);
                break;

            case IngredientType.Ultimate:
                StartCoroutine(UltimatePenaltyRoutine());
                Debug.Log("Ultimate effect: " + ingredient.name);
                break;
        }

        clockTime = Mathf.Max(clockTime, 0f);
    }
}