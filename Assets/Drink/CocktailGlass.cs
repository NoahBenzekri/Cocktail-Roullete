using System.Collections.Generic;
using UnityEngine;

public class CocktailGlass : Interactable
{
    public int maxStack = 4;
    public bool hasCatalyst;
    public List<IngredientsOBJ> ingredientInGlass = new List<IngredientsOBJ>();
    public DrinkEffectType finalEffect = DrinkEffectType.None;

    [SerializeField] private Material clearGlassMaterial;
    public MeshRenderer drinkHolderRender;
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
        Debug.Log($"Adding ingredient: {ingredient.name}");


        if (ingredient.material == null)
        {
            Debug.LogWarning($"{ingredient.name} has no material assigned!");
            return;
        }

        ingredientInGlass.Add(ingredient);
        drinkHolderRender.material = ingredient.material;

        Debug.Log($"Assigning material: {ingredient.material.name}");

       

        UpdateFinalEffect();
    }
    public void ClearGlass()
    {
        ingredientInGlass.Clear();
        finalEffect = DrinkEffectType.None;
        hasCatalyst = false;

        drinkHolderRender.material = clearGlassMaterial;
    }

    void UpdateFinalEffect()
    {
        finalEffect = DrinkEffectType.None;
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

            int priority = GetPriority(effect);

            if (priority > bestPriority)
            {
                bestPriority = priority;
                finalEffect = effect;
            }
        }

        int GetPriority(DrinkEffectType effect)
        {
            switch (effect)
            {
                case DrinkEffectType.Blackout:
                    return 100;

                case DrinkEffectType.LiquidLuck:
                    return 50;

                case DrinkEffectType.Acid:
                    return 40;

                case DrinkEffectType.FrostBite:
                    return 30;

                case DrinkEffectType.Venom:
                    return 20;

                case DrinkEffectType.Catalyst:
                    return 0;

                default:
                    return 0;
            }
        }
    }
}