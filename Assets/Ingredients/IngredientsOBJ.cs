using UnityEngine;

[CreateAssetMenu(fileName = "New Ingredient", menuName = "Cocktail/Ingredient")]
public class IngredientsOBJ: ScriptableObject
{
    public string ingredientName;
    public string description;
    public float amount; 
    public Material material;
    public IngredientType ingredientType;
    public DrinkEffectType effectType;
    public DrinkEffectType CounterEffectType;
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
public enum DrinkEffectType
{
    Catalyst,
    Venom,
    FrostBite,
    Acid,
    LiquidLuck,
    Blackout,
    None

}