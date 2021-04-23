using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class FollowCamera : MonoBehaviour
{
    private CinemachineInputProvider InputProvider;
    private CinemachineFreeLook _FollowCamera;
    [SerializeField]
    private float ZoomSpeed = 5f;
    [SerializeField]
    private float MaxZoomFishFocusMiddleRadious = 7f;
    [SerializeField]
    private float MaxZoomFishFocusTopBottomRadious = 5f;
    [SerializeField]
    private float MinZoomFishFocusMiddleRadious = 7f;
    [SerializeField]
    private float MinZoomFishFocusTopBottomRadious = 5f;
    [SerializeField]
    private float MaxZoomTankFocusMiddleRadious = 20f;
    [SerializeField]
    private float MaxZoomTankFocusTopBottomRadious = 18f;
    [SerializeField]
    private float MinZoomTankFocusMiddleRadious = 20f;
    [SerializeField]
    private float MinZoomTankFocusTopBottomRadious = 18f;
    [SerializeField]
    private InputActionReference AxisControl;
    [SerializeField]
    private GameObject Center;
    [SerializeField]
    private Player PlayerState;
    private bool isActive;
    bool FishInFocus;
    float FishZoomDifference, TankZoomDifference;

    private void Awake()
    {
        InputProvider = GetComponent<CinemachineInputProvider>();
        _FollowCamera = GetComponent<CinemachineFreeLook>();
    }

    void Start()
    {
        isActive = true;
        _FollowCamera.m_Follow = Center.transform;
        _FollowCamera.m_LookAt = Center.transform;
        FishZoomDifference = (MaxZoomFishFocusTopBottomRadious - MinZoomFishFocusTopBottomRadious) / (MaxZoomFishFocusMiddleRadious - MinZoomFishFocusMiddleRadious);
        TankZoomDifference = (MaxZoomTankFocusTopBottomRadious - MinZoomTankFocusTopBottomRadious) / (MaxZoomTankFocusMiddleRadious - MinZoomTankFocusMiddleRadious);
        FishInFocus = false;
        FocusFish(false);
    }

    void Update()
    {
        if (isActive) {
            //Zoom
            if (EventSystem.current.IsPointerOverGameObject()) return;
            float z = InputProvider.GetAxisValue(2);
            if (z != 0) ZoomInZoomOut(z);
        }
    }

    public void OnLeftClick(InputAction.CallbackContext context)
    {
        if (!PlayerState.IsUION() && isActive && context.performed && !EventSystem.current.IsPointerOverGameObject())
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit hit;
            int layer_mask = LayerMask.GetMask("Tier1", "Tier2", "Tier3");
            if (Physics.SphereCast(ray, 0.1f, out hit, Mathf.Infinity, layer_mask))
            {
                var fish = hit.transform.GetComponent<SFlockUnit>();
                if (fish)
                {
                    _FollowCamera.m_Follow = hit.transform;
                    _FollowCamera.m_LookAt = hit.transform;
                    FocusFish(true);
                    PlayerState.IsLookingAtFishState(true, fish);
                }
            }
        }
    }

    public void FollowFish(SFlockUnit fish) {
        _FollowCamera.m_Follow = fish.MyTransform;
        _FollowCamera.m_LookAt = fish.MyTransform;
        FocusFish(true);
        PlayerState.IsLookingAtFishState(true, fish);
    }

    public void CycleLeft(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            SFlockUnit fish = PlayerState.GoToNextFishInFlock(true);
            _FollowCamera.m_Follow = fish.MyTransform;
            _FollowCamera.m_LookAt = fish.MyTransform;
            FocusFish(true);
        }
    }

    public void CycleRight(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            SFlockUnit fish = PlayerState.GoToNextFishInFlock(false);
            _FollowCamera.m_Follow = fish.MyTransform;
            _FollowCamera.m_LookAt = fish.MyTransform;
            FocusFish(true);
        }
    }

    public void ReturnCamera(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _FollowCamera.m_Follow = Center.transform;
            _FollowCamera.m_LookAt = Center.transform;
            FocusFish(false);
            PlayerState.IsLookingAtFishState(false, null);
        }
    }

    public void DisableEnableCameraInput(InputAction.CallbackContext context)
    {
        if (isActive) {
            if (context.performed) {
                InputProvider.XYAxis = AxisControl;
                Cursor.lockState = CursorLockMode.Locked;
            } else if (context.canceled) {
                InputProvider.XYAxis = null;
                Cursor.lockState = CursorLockMode.None;
            }
        }
    }

    public void ZoomInZoomOut(float increment)
    {
        float topOrbit = _FollowCamera.m_Orbits[0].m_Radius;
        float middleOrbit = _FollowCamera.m_Orbits[1].m_Radius;
        float targetTop, targetMiddle;
        if (FishInFocus)
        {
            targetTop = Mathf.Clamp(topOrbit + (increment * FishZoomDifference), MinZoomFishFocusTopBottomRadious, MaxZoomFishFocusTopBottomRadious);
            targetMiddle = Mathf.Clamp(middleOrbit + (increment * FishZoomDifference), MinZoomFishFocusMiddleRadious, MaxZoomFishFocusMiddleRadious);
        }
        else
        {
            targetTop = Mathf.Clamp(topOrbit + (increment * TankZoomDifference), MinZoomTankFocusTopBottomRadious, MaxZoomTankFocusTopBottomRadious);
            targetMiddle = Mathf.Clamp(middleOrbit + (increment * TankZoomDifference), MinZoomTankFocusMiddleRadious, MaxZoomTankFocusMiddleRadious);
        }
        _FollowCamera.m_Orbits[0].m_Radius = Mathf.Lerp(topOrbit, targetTop, ZoomSpeed * Time.unscaledDeltaTime);
        _FollowCamera.m_Orbits[1].m_Radius = Mathf.Lerp(middleOrbit, targetMiddle, ZoomSpeed * Time.unscaledDeltaTime);
        _FollowCamera.m_Orbits[2].m_Radius = Mathf.Lerp(topOrbit, targetTop, ZoomSpeed * Time.unscaledDeltaTime);
    }

    public void FocusFish(bool state)
    {
        if (state)
        {
            _FollowCamera.m_Orbits[0].m_Radius = MaxZoomFishFocusTopBottomRadious;
            _FollowCamera.m_Orbits[1].m_Radius = MaxZoomFishFocusMiddleRadious;
            _FollowCamera.m_Orbits[2].m_Radius = MaxZoomFishFocusTopBottomRadious;
            FishInFocus = true;
        }
        else
        {
            _FollowCamera.m_Orbits[0].m_Radius = MaxZoomTankFocusTopBottomRadious;
            _FollowCamera.m_Orbits[1].m_Radius = MaxZoomTankFocusMiddleRadious;
            _FollowCamera.m_Orbits[2].m_Radius = MaxZoomTankFocusTopBottomRadious;
            FishInFocus = false;
        }
    }

    public bool GetCurrentState() {
        return isActive;
    }

    public void ChangeState(bool state) {
        isActive = state;
    }

    //from 1 to 10
    public void ChangeRotationSpeed(float speed) {
        _FollowCamera.m_XAxis.m_MaxSpeed = speed;
        _FollowCamera.m_YAxis.m_MaxSpeed = speed/0.01f;
    }

    //from 1 to 10
    public void ChangeZoomSpeed(float speed) {
        ZoomSpeed = speed;
    }
}
