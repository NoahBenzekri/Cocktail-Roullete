using UnityEngine;
using DG.Tweening;

public class PlayerLook : MonoBehaviour
{
    [SerializeField] private float lookSensitivity = 0.2f;

    private float pitchX = 0f;
    private float yawY = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;

        pitchX -= mouseY; // subtract so moving mouse up looks up
        yawY += mouseX;

        pitchX = Mathf.Clamp(pitchX, -45f, 45f);
        yawY = Mathf.Clamp(yawY, -15f, 15f); // remove this if you want full horizontal rotation

        transform.localRotation = Quaternion.Euler(pitchX, yawY, 0f);

    }
}