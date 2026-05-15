using UnityEngine;

// Implements the interface directly as a MonoBehaviour component
public class HumanBrain : MonoBehaviour, IPlayerBrain
{
    // Explicit backing field to store the property state
    [SerializeField] private bool isDrinking = false;

    // Implementation of the interface property
    public bool IsDrinking
    {
        get => isDrinking;
        set => isDrinking = value;
    }

    // Implementation of the interface contract method
    public void TakeAction()
    {
        IsDrinking = true;
    }
}