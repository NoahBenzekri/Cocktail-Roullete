using System.Collections.Generic;
using UnityEngine;

public class CocktailGlass : Interactable
{
    public int maxStack = 4;
    public bool hasCatalyst;
    public List<IngredientsOBJ> ingredientInGlass = new List<IngredientsOBJ>();
    public DrinkEffectType finalEffect = DrinkEffectType.None;
    public IngredientsOBJ finalIngredient;

    public override void Interact()
    {
        UpdateFinalEffect();
    }
    public bool HasIngredients()
    {
        return ingredientInGlass.Count > 0;
    }
    public void AddIngredient(IngredientsOBJ ingredient)
    {
        if (ingredientInGlass.Count >= maxStack)
            return;

        ingredientInGlass.Add(ingredient);

        UpdateFinalEffect();
    }

    public void ClearGlass()
    {
        ingredientInGlass.Clear();

        finalEffect = DrinkEffectType.None;
        finalIngredient = null;
    }

  void UpdateFinalEffect()
    {
        finalEffect = DrinkEffectType.None;
        finalIngredient = null;
        hasCatalyst = false;

        int bestPriority = 0;

        foreach (IngredientsOBJ ingredient in ingredientInGlass)
        {
            DrinkEffectType effect = ingredient.effectType;

            if (effect == DrinkEffectType.Catalyst)
            {
                hasCatalyst = true;
                continue;
            }

            // skip if another ingredient counters this one
            bool countered = false;
            foreach (IngredientsOBJ other in ingredientInGlass)
            {
                if (other == ingredient) continue;
                if (other.effectType == ingredient.CounterEffectType)
                {
                    countered = true;
                    Debug.Log(ingredient.name + " countered by " + other.name);
                    break;
                }
            }
            if (countered) continue;

            int priority = GetPriority(effect);

            if (priority > bestPriority)
            {
                bestPriority = priority;
                finalEffect = effect;
                finalIngredient = ingredient;
            }
        }

        int GetPriority(DrinkEffectType effect)
        {
            switch (effect)
            {
                case DrinkEffectType.Blackout:   return 100;
                case DrinkEffectType.LiquidLuck: return 50;
                case DrinkEffectType.Acid:       return 40;
                case DrinkEffectType.FrostBite:  return 30;
                case DrinkEffectType.Venom:      return 20;
                case DrinkEffectType.Catalyst:   return 0;
                default:                         return 0;
            }
        }
    }
}