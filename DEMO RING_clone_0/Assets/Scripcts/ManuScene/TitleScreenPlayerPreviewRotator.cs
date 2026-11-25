using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleScreenPlayerPreviewRotator : MonoBehaviour
{
    PlayerControls inputs;

    private Vector2 cameraInput;
    [SerializeField] private float rotationSpeed = 150f;
    private float horizontalInput;
    private float lookAngle;

    private void Awake()
    {
        if (inputs == null)
        {
            inputs = new PlayerControls();

            inputs.PlayerCamera.Movement.performed += ctx => cameraInput = ctx.ReadValue<Vector2>();
        }
        inputs.Enable();
    }

    void Update()
    {
        horizontalInput = cameraInput.x;
        lookAngle += horizontalInput * rotationSpeed * Time.deltaTime;

        Vector3 rotation = Vector3.zero;
        rotation.y = lookAngle;
        transform.rotation = Quaternion.Euler(rotation);
    }

}
