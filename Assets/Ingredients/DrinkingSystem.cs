using UnityEngine;

public class DrinkingSystem : MonoBehaviour
{
    public CocktailGlass cocktailGlass;

    public void Drink()
    {
        if(cocktailGlass == null)
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
                Debug.Log("Positive effect: " + ingredient.name);
                break;

            case IngredientType.Negative:
                Debug.Log("Negative effect: " + ingredient.name);
                break;

            case IngredientType.Illusion:
                Debug.Log("Illusion effect: " + ingredient.name);
                break;

            case IngredientType.Special:
                Debug.Log("Special effect: " + ingredient.name);
                break;

            case IngredientType.Reverse:
                Debug.Log("Reverse effect: " + ingredient.name);
                break;

            case IngredientType.Ultimate:
                Debug.Log("Ultimate effect: " + ingredient.name);
                break;
        }
    }
}