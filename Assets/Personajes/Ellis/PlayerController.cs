using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [Header("Player Settings")]
    public float walkSpeed = 2f;
    public float runSpeed = 5f;
    public float jumpForce = 5f;
    public float gravity = -9.81f;
    public float rotationSpeed = 10f; // velocidad de rotación con el look

    private CharacterController controller;
    private Animator animator;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool isRunning;
    private bool isJumping;
    private float verticalVelocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        Move();
        Rotate();
        ApplyGravity();
        UpdateAnimator();
    }

    // --- Input System events ---

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        isRunning = context.ReadValueAsButton();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && controller.isGrounded)
        {
            isJumping = true;
            verticalVelocity = jumpForce;
            animator.SetBool("IsJumping", true);
        }
    }

    // --- Movimiento y animación ---

    void Move()
    {
        float targetSpeed = isRunning ? runSpeed : walkSpeed;
        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);

        if (move.magnitude > 0.1f)
        {
            move = transform.TransformDirection(move) * targetSpeed;
        }
        else
        {
            move = Vector3.zero;
        }

        controller.Move(move * Time.deltaTime);
    }

    void Rotate()
    {
        // Rotar personaje según el movimiento (solo si se mueve)
        if (moveInput.sqrMagnitude > 0.1f)
        {
            Vector3 direction = new Vector3(moveInput.x, 0, moveInput.y);
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    void ApplyGravity()
    {
        if (controller.isGrounded)
        {
            if (isJumping)
            {
                isJumping = false;
                animator.SetBool("IsJumping", false);
            }

            if (verticalVelocity < 0)
                verticalVelocity = -1f;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        controller.Move(Vector3.up * verticalVelocity * Time.deltaTime);
    }

    void UpdateAnimator()
    {
        float horizontalSpeed = new Vector3(controller.velocity.x, 0, controller.velocity.z).magnitude;
        animator.SetFloat("Speed", horizontalSpeed / runSpeed);
    }
}
