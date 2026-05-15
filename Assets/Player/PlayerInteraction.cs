using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private LayerMask interactionLayer;
    [SerializeField] private float interactionRange = 10f;

    [Header("Runtime State")]
    public Interactable currentTarget;

    private HumanBrain _humanBrain;

    private void Start()
    {
        _humanBrain = GetComponent<HumanBrain>();

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
    }

    private void Update()
    {
        if (_humanBrain != null && _humanBrain.IsDrinking)
        {
            return;
        }

        LookForInteractable();

        if (Input.GetMouseButtonDown(0) && currentTarget != null)
        {
            currentTarget.Interact();

            if (_humanBrain != null)
            {
                _humanBrain.TakeAction();
            }
        }
    }

    private void LookForInteractable()
    {
        // Always project from the center of the camera view
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Interactable foundTarget = null;

        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange, interactionLayer))
        {
            // Cache reference directly from collider
            foundTarget = hit.collider.GetComponent<Interactable>();

            // Fallback if component is on a parent object
            if (foundTarget == null)
            {
                foundTarget = hit.collider.GetComponentInParent<Interactable>();
            }
        }

        // Target changed state logic
        if (foundTarget != currentTarget)
        {
            currentTarget = foundTarget;

            if (currentTarget != null)
            {
                Debug.Log(currentTarget.name);

                // Safe pattern matching for type casting
                if (currentTarget is Ingredient ingredient && ingredient.ingredientData != null)
                {
                    Debug.Log(ingredient.ingredientData.description);
                }
            }
        }
    }
}