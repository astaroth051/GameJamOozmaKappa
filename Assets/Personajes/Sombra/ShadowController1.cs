using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class ShadowController1 : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;
    private NavMeshAgent agent;
    private Animator animator;
    private SkinnedMeshRenderer[] renderers;
    private Collider shadowCollider;

    [Header("Audio Sources")]
    public AudioSource idleSource;
    public AudioSource walkSource;
    public AudioSource runSource;
    public AudioSource crySource;

    [Header("Clips de Sonido")]
    public AudioClip idleClip;
    public AudioClip walkClip;
    public AudioClip runClip;
    public AudioClip cryClip;

    [Header("Parámetros de comportamiento")]
    public float detectionRadius = 12f;
    public float appearRadius = 14f;
    public float appearDistanceMin = 5f;
    public float appearDistanceMax = 10f;

    [Header("Tiempos de desaparición y aparición")]
    public float fadeDuration = 1.5f;
    public float reappearDelayMin = 10f;
    public float reappearDelayMax = 20f;

    [Header("Intervalos de ataque")]
    public float minCooldown = 8f;
    public float maxCooldown = 15f;

    private bool canAct = true;
    private bool isFading = false;
    private bool isChasing = false;
    private float nextActionDelay = 0f;

    private enum ShadowState { Idle, Walking, Running, Crying }
    private ShadowState currentState;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        renderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        shadowCollider = GetComponent<Collider>();

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        // Empieza la persecución desde el inicio
        StartCoroutine(InitialChaseLoop());
        Debug.Log(" La sombra comenzó la persecución desde el inicio.");
    }

    private void Update()
    {
        if (player == null || isFading) return;

        Vector3 dir = player.position - transform.position;
        dir.y = 0;
        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion rot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 3f);
        }
    }

    private IEnumerator InitialChaseLoop()
    {
        yield return new WaitForSeconds(2f); // pequeña pausa inicial
        StartCoroutine(RandomBehaviorLoop());
    }

    private IEnumerator RandomBehaviorLoop()
    {
        while (true)
        {
            if (canAct && !isFading)
            {
                if (nextActionDelay > 0)
                {
                    yield return new WaitForSeconds(nextActionDelay);
                    nextActionDelay = 0f;
                }

                // La sombra alterna entre moverse alrededor y atacar
                int roll = Random.Range(0, 10);
                if (roll < 2) yield return StartCoroutine(WalkAroundPlayer());
                else yield return StartCoroutine(RunTowardsPlayer());

                nextActionDelay = Random.Range(minCooldown, maxCooldown);
            }
            yield return null;
        }
    }

    private IEnumerator WalkAroundPlayer()
    {
        if (player == null) yield break;

        Vector3 randomDir = Random.insideUnitSphere * detectionRadius;
        randomDir += player.position;
        if (NavMesh.SamplePosition(randomDir, out NavMeshHit hit, detectionRadius, NavMesh.AllAreas))
        {
            agent.isStopped = false;
            agent.speed = 1.5f;
            agent.SetDestination(hit.position);
            TriggerState(ShadowState.Walking);
        }

        float timer = Random.Range(3f, 6f);
        while (timer > 0f)
        {
            timer -= Time.deltaTime;
            yield return null;
        }

        TriggerState(ShadowState.Idle);
    }

    private IEnumerator RunTowardsPlayer()
    {
        if (player == null) yield break;

        TriggerState(ShadowState.Running);
        agent.isStopped = false;
        agent.speed = 5f;

        float chaseTime = Random.Range(5f, 8f);
        while (chaseTime > 0f)
        {
            agent.SetDestination(player.position);
            chaseTime -= Time.deltaTime;

            // Si toca al jugador, desaparece y reaparece más agresiva
            if (Vector3.Distance(transform.position, player.position) < 1.8f && !isFading)
            {
                Debug.Log(" La sombra tocó al jugador. Desaparecerá y volverá pronto...");
                StartCoroutine(FadeOutAndReappear());
                yield break;
            }
            yield return null;
        }

        TriggerState(ShadowState.Crying);
        yield return new WaitForSeconds(2f);

        if (!isFading)
            StartCoroutine(FadeOutAndReappear());
    }

    public IEnumerator FadeOutAndReappear()
    {
        if (isFading) yield break;
        isFading = true;

        if (shadowCollider) shadowCollider.enabled = false;
        yield return StartCoroutine(Fade(1f, 0f));
        agent.isStopped = true;
        SetRenderersVisible(false);

        float delay = Random.Range(reappearDelayMin, reappearDelayMax);
        yield return new WaitForSeconds(delay);

        AppearNearPlayer();
        yield return StartCoroutine(Fade(0f, 1f));

        SetRenderersVisible(true);
        if (shadowCollider) shadowCollider.enabled = true;
        isFading = false;

        Debug.Log(" La sombra reapareció y reinició la persecución.");
        StartCoroutine(RunTowardsPlayer());
    }

    public void AppearNearPlayer()
    {
        if (player == null) return;

        Vector3 randomDir = Random.insideUnitSphere * appearRadius;
        randomDir += player.position;

        if (NavMesh.SamplePosition(randomDir, out NavMeshHit hit, appearRadius, NavMesh.AllAreas))
            transform.position = hit.position;
    }

    private void TriggerState(ShadowState state)
    {
        currentState = state;
        animator.ResetTrigger("Idle");
        animator.ResetTrigger("Walking");
        animator.ResetTrigger("Running");
        animator.ResetTrigger("Crying");
        StopAllAudio();

        switch (state)
        {
            case ShadowState.Idle:
                animator.SetTrigger("Idle");
                PlaySound(idleSource, idleClip, 0.7f);
                break;
            case ShadowState.Walking:
                animator.SetTrigger("Walking");
                PlaySound(walkSource, walkClip, 0.8f);
                break;
            case ShadowState.Running:
                animator.SetTrigger("Running");
                PlaySound(runSource, runClip, 1f);
                break;
            case ShadowState.Crying:
                animator.SetTrigger("Crying");
                PlaySound(crySource, cryClip, 0.9f);
                break;
        }
    }

    private void StopAllAudio()
    {
        if (idleSource) idleSource.Stop();
        if (walkSource) walkSource.Stop();
        if (runSource) runSource.Stop();
        if (crySource) crySource.Stop();
    }

    private void SetRenderersVisible(bool visible)
    {
        foreach (var rend in renderers)
            rend.enabled = visible;
    }

    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float time = 0;
        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float a = Mathf.Lerp(startAlpha, endAlpha, time / fadeDuration);
            SetAlpha(a);
            yield return null;
        }
        SetAlpha(endAlpha);
    }

    private void SetAlpha(float alpha)
    {
        foreach (var rend in renderers)
        {
            if (rend.material.HasProperty("_Color"))
            {
                Color c = rend.material.color;
                c.a = alpha;
                rend.material.color = c;
            }
        }
    }

    private void PlaySound(AudioSource source, AudioClip clip, float volume)
    {
        if (source == null || clip == null) return;
        source.clip = clip;
        source.volume = volume;
        source.loop = true;
        source.Play();
    }
}
