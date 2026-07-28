using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInputHandler playerInput;
    [SerializeField] private PlayerReferences references;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;

    [Header("Jump")]
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -20f;

    [Header("Mouse")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float maxLookAngle = 80f;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float groundRadius = 0.25f;

    private CharacterController controller;

    private float verticalVelocity;
    private float cameraPitch;

    private bool isGrounded;
    

    private void Awake()
    {
        controller = references.CharacterController;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        GroundCheck();


        HandleLook();
        HandleMovement();
        HandleJump();
        HandleGravity();
    }

    void GroundCheck()
    {
        
        isGrounded = Physics.CheckSphere(
            references.GroundCheck.position,
            groundRadius,
            groundMask);

        if (isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
        }

        Debug.Log(isGrounded);
    }

    void HandleMovement()
    {
        Vector2 input = playerInput.Move;

        Vector3 move =
            transform.right * input.x +
            transform.forward * input.y;

        float speed = playerInput.SprintHeld ? sprintSpeed : walkSpeed;

        controller.Move(move * speed * Time.deltaTime);
    }

    void HandleGravity()
    {
        verticalVelocity += gravity * Time.deltaTime;

        controller.Move(Vector3.up * verticalVelocity * Time.deltaTime);
    }

    void HandleJump()
    {
        if (playerInput.JumpPressed)
            Debug.Log("Space Pressed");

        if (!isGrounded)
            return;

        if (!playerInput.JumpPressed)
            return;

        Debug.Log("Jump!");

        verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

        playerInput.ConsumeJump();
    }

    void HandleLook()
    {
        Vector2 look = playerInput.Look * mouseSensitivity * Time.deltaTime;

        // Rotate player left/right
        transform.Rotate(0f, look.x, 0f);

        // Rotate camera up/down
        cameraPitch -= look.y;
        cameraPitch = Mathf.Clamp(cameraPitch, -maxLookAngle, maxLookAngle);

        references.CameraPivot.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
    }

    private void OnDrawGizmosSelected()
    {
        if (references == null || references.GroundCheck == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(references.GroundCheck.position, groundRadius);
    }
}