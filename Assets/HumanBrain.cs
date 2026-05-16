using UnityEngine;

// Implements the interface directly as a MonoBehaviour component
public class HumanBrain : MonoBehaviour, IPlayerBrain
{
    public PlayerInteraction playerInteraction;
    public DrinkingSystem drinkingSystem;

    private bool IsDrinking { get; set; }

    private void Update()
    {
       HandleInteraction();
       HandleDrinking();
    }
    public void HandleInteraction()
    {
        if(IsDrinking) return; // Prevent interaction while drinking
        
        playerInteraction.Tick();

        if (Input.GetMouseButtonDown(0)) 
            OnInteract();
    }

    public void HandleDrinking()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            OnDrink();
        }
    }
    public void OnInteract()
    {
        playerInteraction.TryInteract();
    }
    public void OnDrink()
    {
        IsDrinking = true;
        drinkingSystem.Drink();
        IsDrinking = false;
      
    }
    
}