using UnityEngine;

public class HumanBrain : MonoBehaviour
{
    [Header("References")]
    public PlayerInteraction playerInteraction;
    public DrinkingSystem drinkingSystem;

    [Header("Input")]
    public KeyCode drinkKey = KeyCode.F;

    private bool isDrinking;

    private void Awake()
    {
        if (playerInteraction == null)
            playerInteraction = GetComponent<PlayerInteraction>();

        if (drinkingSystem == null)
            drinkingSystem = GetComponent<DrinkingSystem>();
    }

    private void Update()
    {
        if (isDrinking)
            return;

        HandleInteraction();
        HandleDrinking();
    }

    private void HandleInteraction()
    {
        if (playerInteraction == null)
        {
            Debug.LogError("Missing PlayerInteraction.");
            return;
        }

        playerInteraction.Tick();

        if (Input.GetMouseButtonDown(0))
        {
            playerInteraction.TryInteract();
        }
    }

    private void HandleDrinking()
    {
        if (!Input.GetKeyDown(drinkKey))
            return;

        if (drinkingSystem == null)
        {
            Debug.LogError("Missing DrinkingSystem.");
            return;
        }

        isDrinking = true;
        drinkingSystem.Drink();
        isDrinking = false;
    }
}