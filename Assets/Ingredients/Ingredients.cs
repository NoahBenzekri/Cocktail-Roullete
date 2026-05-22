using UnityEngine;

[CreateAssetMenu(fileName = "New Ingredient", menuName = "Cocktail/Ingredient")]
public class Ingredients: ScriptableObject
{
    public string name;
    public string description;
    public float amount; 
    public IngredientType ingredientType;
}

public enum IngredientType
{
    Positive,
    Negative,
    Illusion,
    Special,
    Reverse,
    Ultimate

}
