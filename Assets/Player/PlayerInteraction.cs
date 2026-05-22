using UnityEngine;
using UnityEngine.UI;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float interactionRange = 10f;

    [Header("Runtime State")]
    public Interactable currentTarget;

    private Outline previousOutline;

    private void Start()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
    }

    public void Tick()
    {
        LookForInteractable();
    }

    public void TryInteract()
    {
        if (currentTarget != null)
        {
            currentTarget.Interact();
        }
    }
    private void Update()
    {
        Tick();

        if (Input.GetMouseButtonDown(0) && currentTarget != null)
        {
            TryInteract();
        }
    }


    private void LookForInteractable()
    {
        // Always project from the center of the camera view
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Interactable foundTarget = null;
        Outline currentOutline = null;

        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange))
        {
            // Cache reference directly from collider
            foundTarget = hit.collider.GetComponent<Interactable>() ?? hit.collider.GetComponentInParent<Interactable>();

            if (foundTarget != null)
            {
                currentOutline = foundTarget.GetComponent<Outline>() ?? foundTarget.GetComponentInParent<Outline>();

            }
        }

        // Target changed state logic
        if (foundTarget != currentTarget)
        {
            currentTarget = foundTarget;

            if (currentTarget != null)
            {
                Debug.Log(currentTarget.name);

                DialogueManager.Instance.StartDialogue("This is a cocktail for " + currentTarget.name);


                // Safe pattern matching for type casting
                if (currentTarget is Ingredient ingredient && ingredient.ingredientData != null)
                {
                    Debug.Log(ingredient.ingredientData.description);

                }
            }
            else
            {
                if (previousOutline != null)
                {
                    // Clear the UI cleanly when looking away into empty space
                    DialogueManager.Instance.ClearDialogue();

                }
            }

            if (previousOutline != currentOutline)
            {
                if (previousOutline != null)
                {
                    previousOutline.enabled = false;
                }

                if (currentOutline != null)
                {
                    currentOutline.enabled = true;
                }

                previousOutline = currentOutline;
            }


        }
    }
}