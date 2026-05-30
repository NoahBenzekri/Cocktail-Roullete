using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    [SerializeField] private float lookAmount = 0.5f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        float mouseX = (Input.mousePosition.x / Screen.width - 0.5f ) * lookAmount;
        float mouseY = (Input.mousePosition.y / Screen.height - 0.5f) * lookAmount;

        float clampedX = Mathf.Clamp(-mouseY, 0f, 45f);

        float clampedY = Mathf.Clamp(mouseX, -15f, 15f);

        transform.localRotation = Quaternion.Euler(clampedX, clampedY, 0f);
    }

 
}
