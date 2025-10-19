using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class EllisTankController : MonoBehaviour
{
    private PlayerControl controls;
    private CharacterController controller;
    private Animator animator;

    [Header("Movimiento")]
    public float walkSpeed = 2f;
    public float runSpeed = 4.5f;
    public float rotateSpeed = 120f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.2f;

    private Vector2 moveInput;
    private Vector3 velocity;
    private bool isJumping;
    private bool isRunning;
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
        controls.Player.Run.started += _ => { isRunning = true; Debug.Log("RUN started (Shift / StickPress)"); };
        controls.Player.Run.canceled += _ => { isRunning = false; Debug.Log("RUN canceled"); };
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

        float currentSpeed = isRunning ? runSpeed : walkSpeed;
        Vector3 forward = transform.forward * moveInput.y * currentSpeed;
        controller.Move(forward * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        if (isJumping) return;

        // Log velocidad y dirección
        if (moveInput.magnitude > 0.1f)
        {
            if (isRunning && moveInput.y > 0)
            {
                Debug.Log("Running: velocidad " + currentSpeed + " - trigger RUN");
                PlayImmediate("run");
            }
            else if (moveInput.y > 0)
            {
                Debug.Log("Walking: velocidad " + currentSpeed + " - trigger WALK");
                PlayImmediate("walk");
            }
            else if (moveInput.y < 0)
            {
                Debug.Log("Backwards");
                PlayImmediate("back");
            }
            else if (moveInput.x < 0)
            {
                Debug.Log("Turning Left");
                PlayImmediate("turnLeft");
            }
            else if (moveInput.x > 0)
            {
                Debug.Log("Turning Right");
                PlayImmediate("turnRight");
            }
        }
        else
        {
            Debug.Log("Idle state (sin movimiento)");
            PlayImmediate("idle");
        }
    }

    private void StartJump()
    {
        if (!isJumping)
        {
            isJumping = true;
            jumpTimer = 2.20f;
            Debug.Log("JUMP start → trigger JUMP");
            PlayImmediate("jump");
            Invoke(nameof(ApplyJumpForce), 0.5f);
        }
    }

    private void ApplyJumpForce()
    {
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        Debug.Log("JUMP force aplicada");
    }

    private void HandleJump()
    {
        if (isJumping)
        {
            jumpTimer -= Time.deltaTime;
            if (jumpTimer <= 0)
            {
                isJumping = false;
                Debug.Log("JUMP terminado → vuelve a IDLE");
                PlayImmediate("idle");
            }
        }
    }

    private void PlayImmediate(string trigger)
    {
        if (animator == null)
        {
            Debug.LogWarning("Animator no asignado");
            return;
        }

        if (trigger == lastTrigger) return;

        // Log trigger que se dispara
        Debug.Log("Trigger → " + trigger);

        animator.Rebind();
        animator.Update(0f);

        animator.ResetTrigger("idle");
        animator.ResetTrigger("walk");
        animator.ResetTrigger("back");
        animator.ResetTrigger("turnLeft");
        animator.ResetTrigger("turnRight");
        animator.ResetTrigger("jump");
        animator.ResetTrigger("run");

        animator.SetTrigger(trigger);
        lastTrigger = trigger;
    }
}
