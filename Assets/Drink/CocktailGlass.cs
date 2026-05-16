using System.Collections.Generic;
using UnityEngine;

public class CocktailGlass : Interactable
{
    public int maxStack = 4;
 // maybe change max stack as the progressive when the game moves along 
    public List<IngredientsOBJ> ingredientInGlass = new List<IngredientsOBJ>();

    public override void Interact()
    {
        Debug.Log("clicked on glass !");
    }
    public void AddIngredient(IngredientsOBJ ingredient)
    {
        if (ingredientInGlass.Count >= maxStack)
        {
            Debug.Log("Glass is full!");
            return;
        }

        ingredientInGlass.Add(ingredient);
        Debug.Log("Added " + ingredient.name);
    }

    public void ClearGlass()
    {
        ingredientInGlass.Clear();
        Debug.Log("Cleared the glass");
       // UpdateDrinkColor();
         // clear the glass
    }

    public void UpdateDrinkColor()
    {
        // Todo Later: Update the color of the drink based on the ingredients in the glass
        // gonna use math function for this instead of hardcoding the colors for each ingredient
    } 
    public bool HasIngredients()
    {
        return ingredientInGlass.Count > 0;
    }
}