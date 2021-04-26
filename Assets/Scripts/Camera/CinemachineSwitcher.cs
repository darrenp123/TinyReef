using UnityEngine;
using UnityEngine.InputSystem;

public class CinemachineSwitcher : MonoBehaviour
{
    [SerializeField]
    private InputAction action;
    private Animator animator;
    private bool followCamera = true;

    private void Awake() {
        animator = GetComponent<Animator>();
    }

    private void OnEnable() {
        action.Enable();
    }

    private void OnDisable() {
        action.Disable();
    }

    void Start()
    {
        action.performed += _ => SwitchState();
    }

    private void SwitchState() {
        if (followCamera) {
            animator.Play("FreeCamera");
        } else {
            animator.Play("FollowCamera");
        }
        followCamera = !followCamera;
    }
}
