using System.Security.Cryptography;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private LayerMask interactionLayer;
    [SerializeField] private float interactionRange = 10f;

    [Header("Runtime State")]
    public Interactable currentTarget;
    public Ingredient selectedIngredient;

    private HumanBrain _humanBrain;

    private void Start()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Tick()
    {
        LookForInteractable();
    }
    public void TryInteract()
    {
        if (currentTarget == null) return;

        if (currentTarget is Ingredient ingredient)
        {
            selectedIngredient = ingredient;
            return;
        }

        if (currentTarget is CocktailGlass glass && selectedIngredient != null)
        {
            glass.AddIngredient(selectedIngredient.ingredientData);
            selectedIngredient = null;
        }
    }

    public void SelectedIngredient(Ingredient ingredient)
    {
        selectedIngredient = ingredient;
        Debug.Log("Selected ingredient: " + ingredient.ingredientData.name);
    }
    private void Update()
    {

    }

    private void LookForInteractable()
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        Interactable foundTarget = null;

        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange, interactionLayer))
        {
            foundTarget = hit.collider.GetComponent<Interactable>();

            if (foundTarget == null)
                foundTarget = hit.collider.GetComponentInParent<Interactable>();
        }

        if (foundTarget == currentTarget) return;
        currentTarget = foundTarget;
    }
}