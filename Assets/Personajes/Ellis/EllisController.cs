using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class EllisTankController : MonoBehaviour
{
    private PlayerControl controls;
    private CharacterController controller;
    private Animator animator;

    [Header("Movimiento")]
    public float moveSpeed = 2f;
    public float rotateSpeed = 120f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.2f;

    private Vector2 moveInput;
    private Vector3 velocity;
    private bool isJumping;
    private float jumpTimer;
    private string lastTrigger = "";

    private void Awake()
    {
        controls = new PlayerControl();
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
        controls.Player.Enable();
        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += _ => moveInput = Vector2.zero;
        controls.Player.Jump.performed += _ => StartJump();
    }

    private void OnDisable()
    {
        controls.Player.Disable();
    }

    private void Update()
    {
        Move();
        HandleJump();
    }

    private void Move()
    {
        bool isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0 && !isJumping)
            velocity.y = -2f;

        transform.Rotate(Vector3.up * moveInput.x * rotateSpeed * Time.deltaTime);

        Vector3 forward = transform.forward * moveInput.y * moveSpeed;
        controller.Move(forward * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        if (isJumping) return;

        if (moveInput.y > 0.1f)
            PlayImmediate("walk");
        else if (moveInput.y < -0.1f)
            PlayImmediate("back");
        else if (moveInput.x < -0.1f)
            PlayImmediate("turnLeft");
        else if (moveInput.x > 0.1f)
            PlayImmediate("turnRight");
        else
            PlayImmediate("idle");
    }

    private void StartJump()
    {
        if (!isJumping)
        {
            isJumping = true;
            jumpTimer = 2.20f; // duración total del salto
            PlayImmediate("jump");
            Invoke(nameof(ApplyJumpForce), 0.5f); // aplica fuerza vertical a los 0.45 s
        }
    }

    private void ApplyJumpForce()
    {
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }

    private void HandleJump()
    {
        if (isJumping)
        {
            jumpTimer -= Time.deltaTime;
            if (jumpTimer <= 0)
            {
                isJumping = false;
                PlayImmediate("idle");
            }
        }
    }

    private void PlayImmediate(string trigger)
    {
        if (animator == null || trigger == lastTrigger) return;

        animator.Rebind();
        animator.Update(0f);

        animator.ResetTrigger("idle");
        animator.ResetTrigger("walk");
        animator.ResetTrigger("back");
        animator.ResetTrigger("turnLeft");
        animator.ResetTrigger("turnRight");
        animator.ResetTrigger("jump");

        animator.SetTrigger(trigger);
        lastTrigger = trigger;
    }
}
