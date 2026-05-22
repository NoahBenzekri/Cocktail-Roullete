using UnityEngine;

public class Ingredient : Interactable
{
    public Ingredients ingredientData;
    public CocktailClass cocktailGlass;

    public override void Interact()
    {
        Debug.Log("Liquid was poured!" + ingredientData.name);

        // todo add sound 
        // todod add visual effect and animaiton of pouring 
        // IMPORTANT todo is to add this pour effect to a cocktail glass 
    }
}   