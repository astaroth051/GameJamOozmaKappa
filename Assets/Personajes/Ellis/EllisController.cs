using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class EllisController : MonoBehaviour
{
    private PlayerControl controls;
    private CharacterController controller;
    private Animator animator;

    [Header("Movimiento")]
    public float walkSpeed = 2f;
    public float runSpeed = 4f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;

    [Header("Cámara Orbitante")]
    public Transform cameraPivot;   // Punto donde se ubica la cámara
    public Transform cameraTarget;  // Centro de rotación (ej: cabeza)
    public float distance = 3f;
    public float orbitSpeed = 120f;
    public float verticalClamp = 60f;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private Vector3 velocity;
    private float yaw;
    private float pitch;
    private bool isRunning;
    private bool isGrounded;
    private bool isJumping;

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

        controls.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        controls.Player.Look.canceled += _ => lookInput = Vector2.zero;

        controls.Player.Run.performed += _ => isRunning = true;
        controls.Player.Run.canceled += _ => isRunning = false;

        controls.Player.Jump.performed += _ => Jump();
    }

    private void OnDisable()
    {
        controls.Player.Disable();
    }

    private void Update()
    {
        HandleCameraOrbit();
        Move();
    }

    private void Move()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
            isJumping = false;
        }

        // El movimiento es relativo al personaje (no a la cámara)
        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);
        move = transform.TransformDirection(move);

        float speed = isRunning ? runSpeed : walkSpeed;
        controller.Move(move * speed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // Lógica de animaciones
        if (isJumping) return;

        if (moveInput.y < -0.1f)
        {
            TriggerAnimation("back");
        }
        else if (moveInput.y > 0.1f)
        {
            TriggerAnimation(isRunning ? "run" : "walk");
        }
        else if (moveInput.x < -0.1f)
        {
            TriggerAnimation("turnLeft");
        }
        else if (moveInput.x > 0.1f)
        {
            TriggerAnimation("turnRight");
        }
        else
        {
            TriggerAnimation("idle");
        }
    }

    private void Jump()
    {
        if (isGrounded)
        {
            isJumping = true;
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            TriggerAnimation("jump");
        }
    }

    private void HandleCameraOrbit()
    {
        // Compatible con mouse y stick derecho
        yaw += lookInput.x * orbitSpeed * Time.deltaTime;
        pitch -= lookInput.y * orbitSpeed * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, -verticalClamp, verticalClamp);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 offset = rotation * new Vector3(0, 0, -distance);
        cameraPivot.position = cameraTarget.position + offset;
        cameraPivot.rotation = rotation;
    }

    private void TriggerAnimation(string trigger)
    {
        if (animator == null) return;
        animator.ResetTrigger("idle");
        animator.ResetTrigger("walk");
        animator.ResetTrigger("run");
        animator.ResetTrigger("jump");
        animator.ResetTrigger("back");
        animator.ResetTrigger("turnLeft");
        animator.ResetTrigger("turnRight");
        animator.SetTrigger(trigger);
    }
}
