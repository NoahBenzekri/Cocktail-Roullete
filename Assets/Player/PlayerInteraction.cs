using UnityEngine;
using System.Collections;
using DG.Tweening;
public class PlayerInteraction : MonoBehaviour
{
    [Header("Raycast")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float interactionRange = 10f;

    public TurnManager turnManager;
    [Header("Runtime")]
    public Interactable currentTarget;
    public Ingredient selectedIngredient;

    private Outline currentOutline;

    private void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;
    }

    public void Tick()
    {
        FindTargetUnderMouse();
    }

    public void TryInteract()
    {
        if (currentTarget == null)
        {
            Debug.Log("No target.");
            return;
        }

        if (currentTarget is Ingredient ingredient)
        {
            if (turnManager.Phase == TurnPhase.AddLiquid)
                SelectIngredient(ingredient);
            return;
        }

        if (currentTarget is CocktailGlass glass)
        {
            if (turnManager.Phase == TurnPhase.AddLiquid)
                AddSelectedIngredientToGlass(glass);
            else if (turnManager.Phase == TurnPhase.PlayerChoice || turnManager.Phase == TurnPhase.PlayerForced)
                turnManager.PlayerDrinks();
            return;
        }

        if (currentTarget is Enemy)
        {
            if (turnManager.Phase == TurnPhase.PlayerChoice)
                turnManager.PlayerPasses();
            return;
        }

        currentTarget.Interact();
    }
    private void FindTargetUnderMouse()
    {
        Vector2 scaledMouse = new Vector2(
            Input.mousePosition.x / Screen.width * 440f,
            Input.mousePosition.y / Screen.height * 360f
        );

        Ray ray = playerCamera.ScreenPointToRay(new Vector3(scaledMouse.x, scaledMouse.y, 0f));

        Interactable foundTarget = null;

        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange))
        {
            foundTarget = hit.collider.GetComponent<Interactable>();

            if (foundTarget == null)
                foundTarget = hit.collider.GetComponentInParent<Interactable>();
        }

        if (foundTarget == currentTarget)
            return;

        SetCurrentTarget(foundTarget);
    }

    private void SetCurrentTarget(Interactable newTarget)
    {
        // hover exit on old target
        if (currentTarget != null && currentTarget is Ingredient oldIngredient)
            oldIngredient.OnHoverExit();

        ClearOutline();
        currentTarget = newTarget;

        if (currentTarget == null)
        {
            ClearDialogue();
            return;
        }

        // hover enter on new target
        if (currentTarget is Ingredient newIngredient)
            newIngredient.OnHoverEnter();

        ShowDialogue(currentTarget);
        ShowOutline(currentTarget);
    }

    public bool isPouring = false;
    private void SelectIngredient(Ingredient ingredient)
    {
        if (isPouring) return; // block double pours
        if (ingredient.ingredientData == null)
        {
            Debug.LogError("Ingredient has no ingredientData.");
            return;
        }

        isPouring = true;
        selectedIngredient = ingredient;
        CocktailGlass glass = FindObjectOfType<CocktailGlass>();

        ingredient.OnHoverExit();
        turnManager.ReturnCamera(restoreLook: false);

        StartCoroutine(PourAfterDelay(ingredient, glass, 1f));
    }

    private IEnumerator PourAfterDelay(Ingredient ingredient, CocktailGlass glass, float delay)
    {
        yield return new WaitForSeconds(delay);

        bool completed = false;
        ingredient.Pour(glass, () =>
        {
            if (completed) return;
            completed = true;
            isPouring = false;
            if (glass != null)
                AddSelectedIngredientToGlass(glass);
        });

        float timeout = 6f;
        float timer = 0f;
        while (!completed && timer < timeout)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (!completed)
        {
            completed = true;
            Debug.LogWarning("Pour timed out — forcing completion.");
            isPouring = false;
            if (glass != null)
                AddSelectedIngredientToGlass(glass);
        }
    }
    private void AddSelectedIngredientToGlass(CocktailGlass glass)
    {
        if (selectedIngredient == null) return;

        glass.AddIngredient(selectedIngredient.ingredientData);
        selectedIngredient = null;

        if (turnManager != null)
            turnManager.PlayerConfirmed();
    }

    private void ShowDialogue(Interactable target)
    {
        if (DialogueManager.Instance == null)
            return;

        if (target is Enemy enemy)
        {
            return;
        }
        if (target is Ingredient ingredient && ingredient.ingredientData != null)
        {
            DialogueManager.Instance.StartDialogue(
                ingredient.ingredientData.ingredientName + "\n" +
                ingredient.ingredientData.description
            );
            return;
        }

        if (target is CocktailGlass)
        {
            if (turnManager.Phase == TurnPhase.PlayerChoice || turnManager.Phase == TurnPhase.PlayerForced)
                DialogueManager.Instance.StartDialogue("Cocktail Glass.");
            else
                DialogueManager.Instance.ClearDialogue();
            return;
        }

        DialogueManager.Instance.StartDialogue(target.name);
    }

    private void ClearDialogue()
    {
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.ClearDialogue();
    }

    private void ShowOutline(Interactable target)
    {
        if (target is Enemy && turnManager.Phase != TurnPhase.PlayerChoice)
            return;

        if (target is CocktailGlass &&
            turnManager.Phase != TurnPhase.PlayerChoice &&
            turnManager.Phase != TurnPhase.PlayerForced)
            return;
        currentOutline = target.GetComponent<Outline>();

        if (currentOutline == null)
            currentOutline = target.GetComponentInParent<Outline>();

        if (currentOutline != null)
            currentOutline.enabled = true;
    }

    private void ClearOutline()
    {
        if (currentOutline != null)
            currentOutline.enabled = false;

        currentOutline = null;
    }
}