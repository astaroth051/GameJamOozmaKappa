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
    private bool isGrounded;
    private bool isJumping;
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
        controls.Player.Jump.performed += _ => Jump();
    }

    private void OnDisable()
    {
        controls.Player.Disable();
    }

    private void Update()
    {
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

        // Rotación tanque
        transform.Rotate(Vector3.up * moveInput.x * rotateSpeed * Time.deltaTime);

        // Avance / retroceso
        Vector3 forward = transform.forward * moveInput.y * moveSpeed;
        controller.Move(forward * Time.deltaTime);

        // Gravedad
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        if (isJumping) return;

        // Selección de animación inmediata
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

    private void Jump()
    {
        if (isGrounded)
        {
            isJumping = true;
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            PlayImmediate("jump");
        }
    }

    private void PlayImmediate(string trigger)
    {
        if (animator == null) return;
        if (trigger == lastTrigger) return; // evita spam innecesario

        animator.Rebind(); // fuerza reset de animación en curso
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
