using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

[RequireComponent(typeof(CharacterController))]
public class EllisTankController : MonoBehaviour
{
    private PlayerControl controls;
    private CharacterController controller;
    private Animator animator;
    private AudioSource audioSource;

    [Header("Movimiento")]
    public float walkSpeed = 2f;
    public float runSpeed = 4.5f;
    public float rotateSpeed = 120f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.2f;

    [Header("Sonidos")]
    public AudioClip walkClip;
    public AudioClip backClip;
    public AudioClip jumpClip;
    public AudioClip pillClip;
    public AudioClip idleLoop;
    public List<AudioClip> focusClips;

    [Header("Tiempos (segundos)")]
    public float walkStepInterval = 0.6f;
    public float runStepInterval = 0.35f;
    public float idleLoopInterval = 4f;
    public float focusInterval = 8f;

    private Vector2 moveInput;
    private Vector3 velocity;
    private bool isJumping;
    private bool isRunning;
    private bool isPilling;
    private bool isFocused;
    private float jumpTimer;
    private float stepTimer;
    private float idleTimer;
    private float focusTimer;
    private string lastTrigger = "";

    private void Awake()
    {
        controls = new PlayerControl();
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void OnEnable()
    {
        controls.Player.Enable();

        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += _ => moveInput = Vector2.zero;

        controls.Player.Jump.performed += _ => StartJump();
        controls.Player.Run.started += _ => isRunning = true;
        controls.Player.Run.canceled += _ => isRunning = false;
        controls.Player.Pill.performed += _ => StartPill();
    }

    private void OnDisable()
    {
        controls.Player.Disable();
    }

    private void Update()
    {
        if (isPilling) return;
        Move();
        HandleJump();
        HandleIdleSound();
        HandleFocusSound();
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

        if (moveInput.y != 0 && isGrounded && !isJumping)
        {
            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0)
            {
                if (moveInput.y > 0)
                    PlayStepSound(walkClip, isRunning);
                else if (moveInput.y < 0)
                    PlayStepSound(backClip, false);

                stepTimer = isRunning ? runStepInterval : walkStepInterval;
            }
        }

        if (isJumping) return;

        if (moveInput.magnitude > 0.1f)
        {
            animator.SetBool("isRunning", isRunning && moveInput.y > 0);

            if (moveInput.y > 0)
                PlayImmediate("walk");
            else if (moveInput.y < 0)
                PlayImmediate("back");
            else if (moveInput.x < 0)
                PlayImmediate("turnLeft");
            else if (moveInput.x > 0)
                PlayImmediate("turnRight");
        }
        else
        {
            animator.SetBool("isRunning", false);
            PlayImmediate("idle");
        }
    }

    private void PlayStepSound(AudioClip clip, bool fast)
    {
        if (clip == null) return;
        audioSource.pitch = fast ? 1.5f : 1f;
        audioSource.PlayOneShot(clip);
    }

    private void StartJump()
    {
        if (!isJumping && !isPilling)
        {
            isJumping = true;
            jumpTimer = 2.20f;
            PlayImmediate("jump");
            Invoke(nameof(ApplyJumpForce), 0.5f);
            if (jumpClip) audioSource.PlayOneShot(jumpClip);
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

    private void StartPill()
    {
        if (isPilling || isJumping) return;

        isPilling = true;
        PlayImmediate("pill");
        if (pillClip) audioSource.PlayOneShot(pillClip);
        Invoke(nameof(EndPill), 6.4f);
    }

    private void EndPill()
    {
        isPilling = false;
        PlayImmediate("idle");
    }

    // --- FOCUS: llamado manualmente desde otra lógica ---
    public void TriggerFocus()
    {
        if (focusClips == null || focusClips.Count == 0) return;
        isFocused = true;
        focusTimer = 0; // ejecuta uno inmediato
    }

    private void HandleFocusSound()
    {
        if (!isFocused) return;

        focusTimer -= Time.deltaTime;
        if (focusTimer <= 0)
        {
            AudioClip clip = focusClips[Random.Range(0, focusClips.Count)];
            audioSource.PlayOneShot(clip);
            focusTimer = focusInterval;
        }
    }

    private void HandleIdleSound()
    {
        if (moveInput.magnitude > 0.1f || isJumping || isPilling) return;

        idleTimer -= Time.deltaTime;
        if (idleTimer <= 0 && idleLoop != null)
        {
            audioSource.PlayOneShot(idleLoop);
            idleTimer = idleLoopInterval;
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
        animator.ResetTrigger("pill");

        animator.SetTrigger(trigger);
        lastTrigger = trigger;
    }
}
