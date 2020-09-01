using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimplePlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5.0f;
    [SerializeField] private float rotationSpeed = 15.0f;
    [SerializeField] private Transform parentTransform;
    [SerializeField] private Transform characterHead;
    [SerializeField] [Range(0, 90)] private float maxRotationX = 90.0f;
    [SerializeField] [Range(-90, 0)] private float minRotationX = -90.0f;
    private Vector2 mouseVector;
    private float headRotX;



    // Start is called before the first frame update
    void Start()
    {
        if (Player.Instance != null)
        {
            PlayerCharacterController controller = Player.Instance.PlayerMovementControls;
            //moveSpeed = controller.WalkSpeed / 2;
            rotationSpeed = controller.LookSensitivity * SettingsMenu.currentMouseSensitivity;
        }
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        LookRotation();
    }



    private void LookRotation()
    {
        mouseVector.x = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
        mouseVector.y = Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;

        headRotX -= mouseVector.y;
        headRotX = Mathf.Clamp(headRotX, minRotationX, maxRotationX);

        characterHead.transform.localRotation = Quaternion.Euler(headRotX, 0f, 0f);
        parentTransform.Rotate(Vector3.up * mouseVector.x);
    }

    private void Move()
    {
        Vector3 movement = parentTransform.forward * Input.GetAxis("Vertical") +
                           parentTransform.right * Input.GetAxis("Horizontal");
        movement = Vector3.ClampMagnitude(movement, 1);

        parentTransform.position += movement * moveSpeed * Time.deltaTime;
    }
}
