using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CocktailGlass : Interactable
{
    public int maxStack = 4;
    public bool hasCatalyst;
    public List<IngredientsOBJ> ingredientInGlass = new List<IngredientsOBJ>();
    public DrinkEffectType finalEffect = DrinkEffectType.None;
    public IngredientsOBJ finalIngredient;

    [Header("Glass Movement")]
    public Transform restPosition;
    public float liftHeight = 0.18f;
    public Ease moveEase = Ease.InOutSine;
    public Vector3 handSocketRotationOffset = new Vector3(-90f, 0f, 0f);

    [Header("Liquid Fill")]
    public Renderer liquidRenderer;
    public float minFillScale = 0.05f;
    public float maxFillScale = 0.2f;

    private MaterialPropertyBlock _mpb;

    void Awake()
    {
        _mpb = new MaterialPropertyBlock();
        if (liquidRenderer != null)
            liquidRenderer.enabled = false;
    }

    public override void Interact() => UpdateFinalEffect();

    public bool HasIngredients() => ingredientInGlass.Count > 0;

    public void AddIngredient(IngredientsOBJ ingredient)
    {
        if (ingredientInGlass.Count >= maxStack) return;
        ingredientInGlass.Add(ingredient);
        UpdateFinalEffect();
        RefreshLiquidVisual();
    }

    public void ClearGlass()
    {
        ingredientInGlass.Clear();
        finalEffect = DrinkEffectType.None;
        finalIngredient = null;
        RefreshLiquidVisual();
    }

    public void MoveGlass(Transform handPosition, float travelDuration, float holdDuration = 1.2f)
    {
        StartCoroutine(MoveGlassRoutine(handPosition, travelDuration, holdDuration));
    }

    public IEnumerator MoveGlassRoutine(Transform handPosition, float travelDuration, float holdDuration = 1.2f)
    {
        if (handPosition == null) yield break;

        Transform originalParent = transform.parent;
        Vector3 originalLocalPos = transform.localPosition;
        Quaternion originalLocalRot = transform.localRotation;
        Vector3 originalWorldPos = transform.position;
        Quaternion originalWorldRot = transform.rotation;

        // 1. Lift
        yield return transform
            .DOMove(transform.position + Vector3.up * liftHeight, travelDuration * 0.2f)
            .SetEase(moveEase)
            .WaitForCompletion();

        // 2. Reparent to hand — IK now carries the glass
        transform.SetParent(handPosition, worldPositionStays: true);

        yield return transform
            .DOLocalMove(Vector3.zero, travelDuration * 0.6f)
            .SetEase(moveEase)
            .WaitForCompletion();

        yield return transform
            .DOLocalRotateQuaternion(Quaternion.Euler(handSocketRotationOffset), travelDuration * 0.2f)
            .SetEase(moveEase)
            .WaitForCompletion();

        // 3. Hold at mouth
        yield return new WaitForSeconds(holdDuration);

        // 4. Reparent back and return
        transform.SetParent(originalParent, worldPositionStays: true);

        if (originalParent != null)
        {
            yield return transform
                .DOLocalMove(originalLocalPos, travelDuration)
                .SetEase(moveEase)
                .WaitForCompletion();

            yield return transform
                .DOLocalRotateQuaternion(originalLocalRot, travelDuration * 0.3f)
                .SetEase(moveEase)
                .WaitForCompletion();
        }
        else
        {
            yield return transform
                .DOMove(originalWorldPos, travelDuration)
                .SetEase(moveEase)
                .WaitForCompletion();

            yield return transform
                .DORotateQuaternion(originalWorldRot, travelDuration * 0.3f)
                .SetEase(moveEase)
                .WaitForCompletion();
        }
    }

    void RefreshLiquidVisual()
    {
        if (liquidRenderer == null) return;

        if (ingredientInGlass.Count == 0)
        {
            liquidRenderer.enabled = false;
            return;
        }

        IngredientsOBJ latest = ingredientInGlass[ingredientInGlass.Count - 1];
        Color targetColor = Color.white;

        if (latest.liquidRenderer != null)
        {
            MaterialPropertyBlock ingMpb = new MaterialPropertyBlock();
            latest.liquidRenderer.GetPropertyBlock(ingMpb);
            targetColor = ingMpb.GetColor("_Color");
            if (targetColor == Color.clear || targetColor == Color.black)
                targetColor = latest.liquidRenderer.sharedMaterial.color;
        }
        else
        {
            targetColor = latest.liquidColor;
        }

        _mpb.SetColor("_TopColor", targetColor);
        _mpb.SetColor("_SideColor", targetColor);

        float fill = (float)ingredientInGlass.Count / maxStack;
        float yScale = Mathf.Lerp(minFillScale, maxFillScale, fill);
        Vector3 s = liquidRenderer.transform.localScale;
        liquidRenderer.transform.localScale = new Vector3(s.x, yScale, s.z);

        liquidRenderer.SetPropertyBlock(_mpb);
        liquidRenderer.enabled = true;
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
            if (effect == DrinkEffectType.Catalyst) { hasCatalyst = true; continue; }

            bool countered = false;
            foreach (IngredientsOBJ other in ingredientInGlass)
            {
                if (other == ingredient) continue;
                if (other.effectType == ingredient.CounterEffectType) { countered = true; break; }
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

        int GetPriority(DrinkEffectType effect) => effect switch
        {
            DrinkEffectType.Blackout => 100,
            DrinkEffectType.LiquidLuck => 50,
            DrinkEffectType.Acid => 40,
            DrinkEffectType.FrostBite => 30,
            DrinkEffectType.Venom => 20,
            _ => 0
        };
    }
}