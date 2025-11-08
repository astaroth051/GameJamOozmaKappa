using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;

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

    [Header("Sonidos")]
    public AudioClip walkLoop;
    public AudioClip backLoop;
    public AudioClip jumpClip;
    public List<AudioClip> pillClips;
    public AudioClip idleLoop;
    public List<AudioClip> focusClips;

    [Header("Duraciones (segundos)")]
    public float pillDuration = 6.4f;
    public float focusInterval = 8f;

    private Vector2 moveInput;
    private Vector3 velocity;
    private bool isJumping;
    private bool isRunning;
    private bool isPilling;
    private bool isFocused;
    private bool isAnxietyFocus;
    private float jumpTimer;
    private float focusTimer;
    private string lastTrigger = "";

    private AudioSource movementAudio;
    private AudioSource actionAudio;
    private AudioSource idleAudio;

    private bool esTercerNivel = false;
    private bool esCuartoNivel = false; //  NUEVO

    private float velocidadBaseWalk;
    private float velocidadBaseRun;

    private void Awake()
    {
        controls = new PlayerControl();
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();

        movementAudio = gameObject.AddComponent<AudioSource>();
        movementAudio.loop = true;
        movementAudio.playOnAwake = false;

        actionAudio = gameObject.AddComponent<AudioSource>();
        actionAudio.loop = false;
        actionAudio.playOnAwake = false;

        idleAudio = gameObject.AddComponent<AudioSource>();
        idleAudio.loop = true;
        idleAudio.playOnAwake = false;

        // Detectar escena actual
        string escenaActual = SceneManager.GetActiveScene().name;
        esTercerNivel = escenaActual == "TercerNivel";
        esCuartoNivel = escenaActual == "CuartoNivel";

        // Guardar velocidades originales
        velocidadBaseWalk = walkSpeed;
        velocidadBaseRun = runSpeed;

        //  Si estamos en CuartoNivel, ralentizamos todo
        if (esCuartoNivel)
        {
            walkSpeed *= 0.4f;
            runSpeed *= 0.4f;

            movementAudio.pitch = 0.4f;
            actionAudio.pitch = 0.4f;
            idleAudio.pitch = 0.4f;

            Debug.Log(" Nivel 4 detectado: movimiento, audio y salto bloqueados.");
        }
    }

    private void OnEnable()
    {
        controls.Player.Enable();

        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += _ => moveInput = Vector2.zero;

        // 🔹 Bloquea salto si está en CuartoNivel
        controls.Player.Jump.performed += _ =>
        {
            if (!esCuartoNivel) StartJump();
            else Debug.Log("⚫ Ellis no puede saltar en este nivel (CuartoNivel).");
        };

        controls.Player.Run.started += _ => isRunning = true;
        controls.Player.Run.canceled += _ => isRunning = false;

        // Bloquear calmantes si estamos en CuartoNivel
        controls.Player.Pill.performed += _ =>
        {
            if (!esCuartoNivel) StartPill();
            else Debug.Log(" Ellis no puede tomar calmantes en este nivel (CuartoNivel).");
        };
    }

    private void OnDisable() => controls.Player.Disable();

    private void Update()
    {
        if (isPilling || isAnxietyFocus) return;
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

        if (isJumping) return;

        if (moveInput.magnitude > 0.1f)
        {
            animator.SetBool("isRunning", isRunning && moveInput.y > 0);

            if (moveInput.y > 0)
                SetMovementLoop(walkLoop, isRunning ? 1.35f : 1f);
            else if (moveInput.y < 0)
                SetMovementLoop(backLoop, 1f);
            else
                StopMovementLoop();

            if (moveInput.y > 0) PlayImmediate("walk");
            else if (moveInput.y < 0) PlayImmediate("back");
            else if (moveInput.x < 0) PlayImmediate("turnLeft");
            else if (moveInput.x > 0) PlayImmediate("turnRight");
        }
        else
        {
            animator.SetBool("isRunning", false);
            StopMovementLoop();
            PlayImmediate("idle");
        }
    }

    private void SetMovementLoop(AudioClip clip, float pitch)
    {
        if (clip == null) return;
        if (movementAudio.clip == clip && movementAudio.isPlaying && Mathf.Approximately(movementAudio.pitch, pitch)) return;

        movementAudio.clip = clip;
        float finalPitch = esCuartoNivel ? pitch * 0.6f : pitch;
        movementAudio.pitch = finalPitch;
        movementAudio.volume = esTercerNivel ? 0.5f : 1f;
        movementAudio.Play();
    }

    private void StopMovementLoop()
    {
        if (movementAudio.isPlaying)
            movementAudio.Stop();
    }

    private void StartJump()
    {
        // 🔹 Bloquear salto en CuartoNivel
        if (esCuartoNivel) return;

        if (!isJumping && !isPilling && !isAnxietyFocus)
        {
            isJumping = true;
            jumpTimer = 2.20f;
            PlayImmediate("jump");
            Invoke(nameof(ApplyJumpForce), 0.5f);

            if (jumpClip)
            {
                float vol = esTercerNivel ? 0.5f : 1f;
                float pitch = esCuartoNivel ? 0.4f : 1f;
                actionAudio.pitch = pitch;
                actionAudio.PlayOneShot(jumpClip, vol);
            }
        }
    }

    private void ApplyJumpForce()
    {
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }

    private void HandleJump()
    {
        if (!isJumping) return;
        jumpTimer -= Time.deltaTime;
        if (jumpTimer <= 0)
        {
            isJumping = false;
            PlayImmediate("idle");
        }
    }

    private void StartPill()
    {
        if (isPilling || isJumping || isAnxietyFocus) return;
        isPilling = true;

        StopMovementLoop();
        if (idleAudio.isPlaying) idleAudio.Stop();

        PlayImmediate("pill");

        Invoke(nameof(TriggerPillEffect), 0.15f);
        PlayPillClips();

        moveInput = Vector2.zero;
        isRunning = false;

        Invoke(nameof(EndPill), pillDuration);
    }

    private void TriggerPillEffect()
    {
        var post = FindObjectOfType<PostProcessController>();
        if (post != null) post.PillEffect();

        var anxiety = FindObjectOfType<AnxietySystem>();
        if (anxiety != null) anxiety.TakePill();
    }

    private void PlayPillClips()
    {
        if (pillClips == null || pillClips.Count == 0) return;
        foreach (var clip in pillClips)
        {
            if (clip == null) continue;
            AudioSource s = gameObject.AddComponent<AudioSource>();
            s.clip = clip;
            s.loop = false;
            s.volume = esTercerNivel ? 0.5f : 1f;
            s.pitch = esCuartoNivel ? 0.4f : 1f;
            s.Play();
            Destroy(s, clip.length + 0.1f);
        }
    }

    private void EndPill()
    {
        isPilling = false;
        PlayImmediate("idle");
    }

    public void TriggerFocus()
    {
        if (focusClips == null || focusClips.Count == 0) return;
        isFocused = true;
        focusTimer = 0;
    }

    private void HandleFocusSound()
    {
        if (!isFocused) return;
        focusTimer -= Time.deltaTime;
        if (focusTimer <= 0)
        {
            foreach (var clip in focusClips)
            {
                if (clip == null) continue;
                GameObject tempGO = new GameObject("TempFocusSound");
                tempGO.transform.position = transform.position;

                AudioSource tempAudio = tempGO.AddComponent<AudioSource>();
                tempAudio.clip = clip;
                tempAudio.volume = esTercerNivel ? 0.5f : 1f;
                tempAudio.pitch = esCuartoNivel ? 0.4f : 1f;
                tempAudio.Play();

                Destroy(tempGO, clip.length / tempAudio.pitch + 0.1f);
            }
            focusTimer = focusInterval;
        }
    }

    private void HandleIdleSound()
    {
        if (moveInput.magnitude > 0.1f || isJumping || isPilling || isAnxietyFocus)
        {
            if (idleAudio.isPlaying) idleAudio.Stop();
            return;
        }

        if (idleLoop != null && !idleAudio.isPlaying)
        {
            idleAudio.clip = idleLoop;
            idleAudio.loop = true;
            idleAudio.volume = esTercerNivel ? 0.5f : 1f;
            idleAudio.pitch = esCuartoNivel ? 0.4f : 1f;
            idleAudio.Play();
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
        animator.ResetTrigger("focus");

        animator.SetTrigger(trigger);
        lastTrigger = trigger;
    }

    public void TriggerAnxietyFocus()
    {
        if (isAnxietyFocus) return;

        isAnxietyFocus = true;
        moveInput = Vector2.zero;
        isRunning = false;
        isPilling = false;

        StopAllCoroutines();
        movementAudio.Stop();
        idleAudio.Stop();

        PlayImmediate("focus");

        if (focusClips != null && focusClips.Count > 0)
        {
            float vol = esTercerNivel ? 0.5f : 1f;
            float pitch = esCuartoNivel ? 0.4f : 1f;
            actionAudio.pitch = pitch;
            actionAudio.PlayOneShot(focusClips[0], vol);
        }

        StartCoroutine(RestartSceneAfterFocus());
    }

    private IEnumerator RestartSceneAfterFocus()
    {
        yield return new WaitForSeconds(5.5f);

        var anxiety = FindObjectOfType<AnxietySystem>();
        if (anxiety != null)
            anxiety.StartCoroutine("FadeToBlack", 2f);

        KeyItem.llaveRecogida = false;
        yield return new WaitForSeconds(1.5f);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
