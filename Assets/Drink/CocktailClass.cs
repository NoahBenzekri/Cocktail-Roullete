using System.Collections.Generic;
using UnityEngine;

public class CocktailClass : Interactable
{
    public int maxStack = 4;
 // maybe change max stack as the progressive when the game moves along 
    public List<Ingredient> ingredientInGlass = new List<Ingredient>();

    public void AddIngredient(Ingredient ingredient)
    {
         //// add ingredients to glass 
    }

    public override void Interact()
    {
        PlayerDrink();
    }

    public void PlayerDrink()
    {
        Debug.Log("Player drank the cocktail!");
    }
    public void PassAlong()
    {
        Debug.Log("Cocktail was passed along!");
    }

    public void EmptyGlass()
    {
        Debug.Log("Glass was emptied!");
    }
    public void PrintCocktailContents()
    {
        Debug.Log("Cocktail contains:");
        foreach (Ingredient ingredient in ingredientInGlass)
        {
            Debug.Log("- " + ingredient.ingredientData.name);
        }
    }
}