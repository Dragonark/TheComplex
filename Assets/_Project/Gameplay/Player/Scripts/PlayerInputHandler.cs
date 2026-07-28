using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    private IA_Player controls;

    public Vector2 Move { get; private set; }
    public Vector2 Look { get; private set; }

    public bool JumpPressed { get; private set; }
    public bool SprintHeld { get; private set; }
    public bool InteractPressed { get; private set; }

    private void Awake()
    {
        controls = new IA_Player();
    }

    private void Update()
    {
        if (JumpPressed)
            Debug.Log("Jump Pressed");
    }

    private void OnEnable()
    {
        controls.Enable();

        controls.Player.Move.performed += ctx => Move = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => Move = Vector2.zero;

        controls.Player.Look.performed += ctx => Look = ctx.ReadValue<Vector2>();
        controls.Player.Look.canceled += ctx => Look = Vector2.zero;

        controls.Player.Jump.performed += ctx => JumpPressed = true;
        controls.Player.Jump.canceled += ctx => JumpPressed = false;

        controls.Player.Sprint.performed += ctx => SprintHeld = true;
        controls.Player.Sprint.canceled += ctx => SprintHeld = false;

        controls.Player.Interact.performed += ctx => InteractPressed = true;
        controls.Player.Interact.canceled += ctx => InteractPressed = false;
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    public void ConsumeJump()
    {
        JumpPressed = false;
    }

    public void ConsumeInteract()
    {
        InteractPressed = false;
    }
}