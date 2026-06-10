using UnityEngine;
<<<<<<< Updated upstream
=======
using DG.Tweening;
using System.Collections;
using Unity.Properties;
>>>>>>> Stashed changes

public class Ingredient : Interactable
{
    public IngredientsOBJ ingredientData;
    public CocktailGlass cocktailGlass;

<<<<<<< Updated upstream
=======
    public Vector3 tiltAxis = new Vector3(1f, 0f, 0f);

    [SerializeField] private float hoverDistance = 0.5f;
    [SerializeField] private float hoverDuration = 0.2f;
    [SerializeField] private float pourHeight = 1.5f;
    [SerializeField] private float pourDuration = 0.5f;
    [SerializeField] private float tiltAngle = 120f;


    private void Awake()
    {
        originalPos = transform.position;
        originalRot = transform.rotation;
    }

    [ContextMenu("Start Pour")]
    public void StartPour()
    {
        StartCoroutine(Pour(cocktailGlass));

    }

    private Sequence activeSequence;

    public void OnHoverEnter()
    {
        if (isHovered) return;
        isHovered = true;
        activeSequence?.Kill();
        Vector3 toCamera = (Camera.main.transform.position - transform.position).normalized;
        activeSequence = DOTween.Sequence();
        activeSequence.Append(transform.DOMove(originalPos + toCamera * hoverDistance, hoverDuration).SetEase(Ease.OutBack));
    }

    public void OnHoverExit()
    {
        if (!isHovered) return;
        isHovered = false;
        activeSequence?.Kill();
        activeSequence = DOTween.Sequence();
        activeSequence.Append(transform.DOMove(originalPos, hoverDuration).SetEase(Ease.InOutSine));
    }

    public void ResetPose()
    {
        activeSequence?.Kill();
        isHovered = false;
        transform.position = originalPos;
        transform.rotation = originalRot;
    }

    public IEnumerator Pour(CocktailGlass glass)
    {
        float elapsed = 0f;

        Vector3 target = pourTarget != null
            ? pourTarget.position
            : new Vector3(glass.transform.position.x, glass.transform.position.y + pourHeight, glass.transform.position.z);

        Quaternion tiltRot = originalRot * Quaternion.Euler(tiltAxis.normalized * tiltAngle);

        // Breakdown pourDuration into: approach (move), rotate (at target), and hold (pour) phases.
        // approach + rotate + hold == pourDuration (if pourDuration is small some phases get minimum values).
        float minPhase = 0.0001f;
        float approachDuration = Mathf.Max(minPhase, pourDuration * 0.25f);
        float rotateDuration = Mathf.Max(minPhase, pourDuration * 0.25f);
        float holdDuration = Mathf.Max(0f, pourDuration - approachDuration - rotateDuration);

        // 1) Move toward target position (no rotation)
        elapsed = 0f;
        while (elapsed < approachDuration)
        {
            isHovered = false;
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / approachDuration);
            transform.position = Vector3.Lerp(originalPos, target, t);
            transform.rotation = originalRot; // keep original rotation while moving
            yield return null;
        }

        // Ensure exact position at target
        transform.position = target;
        transform.rotation = originalRot;

        // 2) Rotate to tilt once at the target position
        elapsed = 0f;
        while (elapsed < rotateDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / rotateDuration);
            transform.rotation = Quaternion.Slerp(originalRot, tiltRot, t);
            yield return null;
        }

        // Ensure exact rotation for pouring
        transform.position = target;
        transform.rotation = tiltRot;

        // 3) Hold (pour) at the tilted pose for the configured duration (stop instead of instant return)
        elapsed = 0f;
        while (elapsed < holdDuration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 4) Move back to original pose (position + rotation) simultaneously over combined duration
        float returnDuration = approachDuration + rotateDuration;
        elapsed = 0f;
        while (elapsed < returnDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / returnDuration);
            transform.position = Vector3.Lerp(target, originalPos, t);
            transform.rotation = Quaternion.Slerp(tiltRot, originalRot, t);
            yield return null;
        }

        // Ensure exact original pose
        transform.position = originalPos;
        transform.rotation = originalRot;
    }
>>>>>>> Stashed changes
    public override void Interact()
    {
        Debug.Log("Liquid was poured!" + ingredientData.name);
        // todo add sound 
        // todod add visual effect and animaiton of pouring 
        // IMPORTANT todo is to add this pour effect to a cocktail glass 
    }
}   