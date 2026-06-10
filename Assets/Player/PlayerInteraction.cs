using UnityEngine;

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
            SelectIngredient(ingredient);
            return;
        }

        if (currentTarget is CocktailGlass glass)
        {
            AddSelectedIngredientToGlass(glass);
            return;
        }

        currentTarget.Interact();
    }

    private void FindTargetUnderMouse()
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);

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
        ClearOutline();

        currentTarget = newTarget;

        if (currentTarget == null)
        {
            ClearDialogue();
            return;
        }

        ShowDialogue(currentTarget);
        ShowOutline(currentTarget);

        Debug.Log("Current target: " + currentTarget.name);
    }

    private void SelectIngredient(Ingredient ingredient)
    {
        if (ingredient.ingredientData == null)
        {
            Debug.LogError("Ingredient has no ingredientData.");
            return;
        }

        selectedIngredient = ingredient;

        Debug.Log("Selected: " + ingredient.ingredientData.ingredientName);
    }

<<<<<<< Updated upstream
=======
    private IEnumerator PourAfterDelay(Ingredient ingredient, CocktailGlass glass, float delay)
{
    yield return new WaitForSeconds(delay);

    bool completed = false;
    yield return ingredient.Pour(glass);

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
>>>>>>> Stashed changes
    private void AddSelectedIngredientToGlass(CocktailGlass glass)
    {
        if (selectedIngredient == null)
        {
            Debug.Log("Pick an ingredient first.");
            return;
        }

        glass.AddIngredient(selectedIngredient.ingredientData);

        Debug.Log("Added " + selectedIngredient.ingredientData.ingredientName + " to glass.");

        selectedIngredient = null;

        if(turnManager != null)
            turnManager.PlayerConfirmed();
    }

    private void ShowDialogue(Interactable target)
    {
        if (DialogueManager.Instance == null)
            return;

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
            DialogueManager.Instance.StartDialogue("Cocktail Glass");
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