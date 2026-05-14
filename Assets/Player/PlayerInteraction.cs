using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public Camera playerCamera;
    public float interactionRange = 10f;

    public Interactable currentTarget;

    void Update()
    {
        LookForInteractable();

        if (Input.GetMouseButtonDown(0) && currentTarget != null)
        {
            currentTarget.Interact();
        }
    }
    void LookForInteractable()
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);

        Interactable foundTarget = null;

        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange))
        {
            foundTarget = hit.collider.GetComponentInParent<Interactable>();
        }
        if (foundTarget != currentTarget)
        {
            currentTarget = foundTarget;
            if (currentTarget != null)
            {
                Debug.Log(currentTarget.name);

                Ingredient ingredient = currentTarget as Ingredient;

                if (ingredient != null)
                {
                    Debug.Log(ingredient.ingredientData.description);
                }
            }
        }
    }
}