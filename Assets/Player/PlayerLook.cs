using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    [SerializeField] private float sensitivity = 100f;

    [SerializeField] private float clampAngle = 40f;

    private float _xRotation;
    private float _yRotation;

    void Start()
    {
        LockCursor(true);
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;



        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -clampAngle, clampAngle);

        _yRotation += mouseX;
        _yRotation = Mathf.Clamp(_yRotation, -clampAngle, clampAngle);  

        transform.localRotation = Quaternion.Euler(_xRotation, _yRotation, 0f);
    }

    public void LockCursor(bool isLocked)
    {
        Cursor.lockState = isLocked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !isLocked;
    }
}