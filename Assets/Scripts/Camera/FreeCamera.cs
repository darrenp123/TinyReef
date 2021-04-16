using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FreeCamera : MonoBehaviour
{

    [SerializeField]
    private float cameraSpeed;
    private float speed;
    [SerializeField]
    private InputAction action;
    private Vector2 moveAxis;

    private void OnEnable() {
        action.Enable();
    }

    private void OnDisable() {
        action.Disable();
    }

    void Start()
    {
        speed = 0f;
        action.performed += MoveCamera;
        action.canceled += StopCamera;
    }

    void Update()
    {
        float x = moveAxis.x * Time.unscaledDeltaTime * speed;
        float y = moveAxis.y * Time.unscaledDeltaTime * speed;
        transform.localPosition += transform.right * x;
        transform.localPosition += transform.forward * y;
    }

    private void MoveCamera(InputAction.CallbackContext context) {
        moveAxis = context.ReadValue<Vector2>();
        speed = cameraSpeed;
    }

    private void StopCamera(InputAction.CallbackContext context) {
        speed = 0;
    }
}
