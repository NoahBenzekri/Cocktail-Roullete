using UnityEngine;
using DG.Tweening;

public class Ingredient : Interactable
{
    public IngredientsOBJ ingredientData;
    public CocktailGlass cocktailGlass;
    private Vector3 originalPos;
    private Quaternion originalRot;
    private bool isHovered = false;
    public Transform pourTarget;

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
    public void Pour(CocktailGlass glass, System.Action onComplete)
    {
        isHovered = false;
        activeSequence?.Kill();

        Vector3 target = pourTarget != null
            ? pourTarget.position
            : new Vector3(glass.transform.position.x, glass.transform.position.y + pourHeight, glass.transform.position.z);

        Quaternion tiltRot = originalRot * Quaternion.Euler(tiltAxis.normalized * tiltAngle);

        activeSequence = DOTween.Sequence();
        activeSequence.Append(transform.DOMove(target, 1.2f).SetEase(Ease.InOutSine));
        activeSequence.Append(transform.DORotateQuaternion(tiltRot, 0.6f).SetEase(Ease.InOutSine));
        activeSequence.AppendInterval(1f);
        activeSequence.Append(transform.DORotateQuaternion(originalRot, 0.6f).SetEase(Ease.InOutSine));
        activeSequence.Append(transform.DOMove(originalPos, 1.2f).SetEase(Ease.InOutSine));
        activeSequence.OnComplete(() =>
        {
            // hard snap to original in case tween drifted
            transform.position = originalPos;
            transform.rotation = originalRot;
            onComplete?.Invoke();
        });
    }
    public override void Interact()
    {
        Debug.Log("Liquid was poured!" + ingredientData.name);
        // todo add sound 
        // todod add visual effect and animaiton of pouring 
        // IMPORTANT todo is to add this pour effect to a cocktail glass 
    }
}