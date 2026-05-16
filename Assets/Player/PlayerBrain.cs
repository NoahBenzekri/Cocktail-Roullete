using UnityEngine;

public class PlayerBrain : MonoBehaviour
{
    public PlayerInteraction playerInteraction;
    public DrinkingSystem drinkingSystem;

    private void Update()
    {
       HandleInteraction();
       HandleDrinking();
    }
    private void HandleInteraction()
    {
        playerInteraction.Tick();

        if (Input.GetKeyDown(KeyCode.E))
            OnInteract();
    }

    private void HandleDrinking()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            drinkingSystem.Drink();
        }
    }
    private void OnInteract()
    {
        playerInteraction.TryInteract();
    }
    private void OnDrink()
    {
        drinkingSystem.Drink();
    }
}