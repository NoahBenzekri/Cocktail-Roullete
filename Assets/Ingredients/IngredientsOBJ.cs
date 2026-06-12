using UnityEngine;

[CreateAssetMenu(fileName = "New Ingredient", menuName = "Cocktail/Ingredient")]
public class IngredientsOBJ: ScriptableObject
{
    public string ingredientName;
    public string description;
    public float amount; 
    public IngredientType ingredientType;
    public DrinkEffectType effectType;
    public DrinkEffectType CounterEffectType;

    [Header("Visual")]
    public Renderer liquidRenderer;   // the renderer on the bottle/ingredient mesh
    public Color liquidColor = Color.white; // fallback if renderer has no property block
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