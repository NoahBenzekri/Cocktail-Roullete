using UnityEngine;
using DG.Tweening;

public class Ingredient : Interactable
{
    public IngredientsOBJ ingredientData;
    public CocktailGlass cocktailGlass;

    private Vector3 originalPos;
    private Quaternion originalRot;
    private bool isHovered;

    public bool IsPouring { get; private set; }

    [Header("Hover")]
    [SerializeField] private float hoverDistance = 0.5f;
    [SerializeField] private float hoverDuration = 0.2f;

    [Header("Pour")]
    [SerializeField] private float pourHeight = 1.5f;
    [SerializeField] private float moveDuration = 0.6f;
    [SerializeField] private float rotateDuration = 0.4f;
    [SerializeField] private float pourDuration = 1f;
    [SerializeField] private float tiltAngle = 120f;

    [Tooltip("Local axis used for tilting the bottle.")]
    public Vector3 tiltAxis = Vector3.right;

    [Header("Pour Visual")]
    [SerializeField] private ParticleSystem pourParticles;

    private Sequence activeSequence;

    private void Awake()
    {
        originalPos = transform.position;
        originalRot = transform.rotation;

        if (pourParticles != null)
            pourParticles.Stop();
    }

    public void OnHoverEnter()
    {
        if (isHovered || IsPouring)
            return;

        isHovered = true;

        activeSequence?.Kill();

        Vector3 toCamera =
            (Camera.main.transform.position - transform.position).normalized;

        activeSequence = DOTween.Sequence();

        activeSequence.Append(
            transform.DOMove(
                originalPos + toCamera * hoverDistance,
                hoverDuration)
            .SetEase(Ease.OutBack));
    }

    public void OnHoverExit()
    {
        if (!isHovered || IsPouring)
            return;

        isHovered = false;

        activeSequence?.Kill();

        activeSequence = DOTween.Sequence();

        activeSequence.Append(
            transform.DOMove(originalPos, hoverDuration)
            .SetEase(Ease.InOutSine));
    }

    public void ResetPose()
    {
        if (IsPouring)
            return;

        activeSequence?.Kill(false);

        isHovered = false;

        transform.SetPositionAndRotation(originalPos, originalRot);

        if (pourParticles != null)
            pourParticles.Stop();
    }

    public void Pour(CocktailGlass glass, System.Action onComplete)
    {
        if (glass == null)
        {
            onComplete?.Invoke();
            return;
        }

        activeSequence?.Kill(false);

        IsPouring = true;
        isHovered = false;

        Vector3 targetPos =
            glass.transform.position + Vector3.up * pourHeight;

        Quaternion tiltRot =
            originalRot *
            Quaternion.AngleAxis(
                tiltAngle,
                tiltAxis.normalized);

        if (pourParticles != null && ingredientData != null)
        {
            var main = pourParticles.main;
            main.startColor = ingredientData.liquidColor;
        }

        activeSequence = DOTween.Sequence();

        activeSequence.Append(
            transform.DOMove(targetPos, moveDuration)
            .SetEase(Ease.InOutSine));

        activeSequence.Append(
            transform.DORotateQuaternion(
                tiltRot,
                rotateDuration)
            .SetEase(Ease.InOutSine));

        activeSequence.AppendCallback(() =>
        {
            if (pourParticles != null)
                pourParticles.Play();
        });

        activeSequence.AppendInterval(pourDuration);

        activeSequence.AppendCallback(() =>
        {
            if (pourParticles != null)
                pourParticles.Stop();
        });

        activeSequence.Append(
            transform.DORotateQuaternion(
                originalRot,
                rotateDuration)
            .SetEase(Ease.InOutSine));

        activeSequence.Append(
            transform.DOMove(originalPos, moveDuration)
            .SetEase(Ease.InOutSine));

        activeSequence.OnKill(() =>
        {
            if (pourParticles != null)
                pourParticles.Stop();

            transform.SetPositionAndRotation(originalPos, originalRot);
            IsPouring = false;
        });

        activeSequence.OnComplete(() =>
        {
            transform.SetPositionAndRotation(originalPos, originalRot);

            IsPouring = false;

            onComplete?.Invoke();
        });
    }

    public override void Interact()
    {
        Debug.Log("Liquid was poured! " + ingredientData.name);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.right);

        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, transform.up);

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.forward);
    }
#endif
}