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


        transform.localRotation = Quaternion.Euler(-mouseY, mouseX, 0f);
    }

 
}