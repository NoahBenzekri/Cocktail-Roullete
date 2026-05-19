using UnityEngine;
using UnityEngine.Events;

public class GenericTrigger : MonoBehaviour
{
    public UnityEvent triggerEvent;
    private void OnTriggerStay(Collider other)
    {
        triggerEvent?.Invoke();

    }
}
