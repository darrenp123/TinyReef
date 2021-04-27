using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class FreeCamera : MonoBehaviour
{

    [SerializeField]
    private float cameraSpeed;
    private float speed;
    [SerializeField]
    private InputAction moveAction;
    [SerializeField]
    private InputAction rotateAction;
    [SerializeField]
    private InputAction leftClick;
    private Vector2 moveAxis;
    private CinemachineInputProvider InputProvider;
    [SerializeField]
    private InputActionReference AxisControl;
    private bool isActive;
    [SerializeField]
    private FollowCamera followCamera;
    [SerializeField]
    private CinemachineSwitcher switcher;
    private CinemachinePOV  _FreeLookCamera;

    private void OnEnable() {
        moveAction.Enable();
        rotateAction.Enable();
        leftClick.Enable();
    }

    private void OnDisable() {
        moveAction.Disable();
        rotateAction.Disable();
        leftClick.Disable();
    }

    void Awake()
    {
        isActive = false;
        speed = 0f;
        moveAction.performed += MoveCamera;
        moveAction.canceled += StopCamera;
        rotateAction.performed += _ => EnableRotation();
        rotateAction.canceled += _ => DisableRotation();
        leftClick.performed += _ => OnLeftClick();
        InputProvider = GetComponent<CinemachineInputProvider>();
        _FreeLookCamera = GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachinePOV>();
    }

    void Update()
    {
        if (isActive) {
            float x = moveAxis.x * Time.unscaledDeltaTime * speed;
            float y = moveAxis.y * Time.unscaledDeltaTime * speed;
            transform.localPosition += transform.right * x;
            transform.localPosition += transform.forward * y;
        }
    }

    private void MoveCamera(InputAction.CallbackContext context) {
        moveAxis = context.ReadValue<Vector2>();
        speed = cameraSpeed;
    }

    private void StopCamera(InputAction.CallbackContext context) {
        speed = 0;
    }

    public void EnableRotation() {
        InputProvider.XYAxis = AxisControl;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void DisableRotation() {
        InputProvider.XYAxis = null;
        Cursor.lockState = CursorLockMode.None;
    }

    public void OnLeftClick() {
        if (isActive && !EventSystem.current.IsPointerOverGameObject()) {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit hit;
            int layer_mask = LayerMask.GetMask("Tier1", "Tier2", "Tier3");
            if (Physics.SphereCast(ray, 0.1f, out hit, Mathf.Infinity, layer_mask)) {
                var fish = hit.transform.GetComponent<SFlockUnit>();
                if (fish) {
                    isActive = false;
                    switcher.SwitchState();
                    followCamera.FollowFish(fish);
                }
            }
        }
    }

    public bool GetCurrentState() {
        return isActive;
    }

    public void ChangeState(bool state) {
        isActive = state;
    }

    //from 100 to 300
    public void ChangeRotationSpeed(float speed) {
        print("Free rotation" + speed);
        _FreeLookCamera.m_VerticalAxis.m_MaxSpeed = speed;
        _FreeLookCamera.m_HorizontalAxis.m_MaxSpeed = speed;
        PlayerPrefs.SetFloat("FreeCamSens", speed);
        PlayerPrefs.Save();
    }

    //from 10 to 30
    public void ChangeCameraSpeed(float speed) {
        print("Free speed" + speed);
        cameraSpeed = speed;
        PlayerPrefs.SetFloat("FreeCamMoveSpeed", speed);
        PlayerPrefs.Save();
    }
}
