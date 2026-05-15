
using UnityEngine;

public class AiBrain : IPlayerBrain
{
    [SerializeField] private bool isDrinking = false;
    public bool IsDrinking
    {
        get => isDrinking;
        set => isDrinking = value;
    }
    public void TakeAction()
    {
        IsDrinking = true;
    }
}
