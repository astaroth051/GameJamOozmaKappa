using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class ShadowController : MonoBehaviour
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
    public float detectionRadius = 10f;
    public float appearRadius = 15f;
    public float appearDistanceMin = 6f;
    public float appearDistanceMax = 12f;

    [Header("Tiempos de desaparición y aparición")]
    public float fadeDuration = 2f;
    public float reappearDelayMin = 20f;
    public float reappearDelayMax = 40f;
    private bool isFading = false;

    [Header("Intervalos de ataque")]
    public float minCooldown = 15f;
    public float maxCooldown = 60f;

    private bool firstSeen = false;
    private bool canAct = false;
    private float nextActionDelay = 0f;
    private bool recentlyTouched = false;

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

        canAct = true;
        StartCoroutine(RandomBehaviorLoop());

        Debug.Log(" La sombra está en escena, esperando ser vista por primera vez...");
    }

    private void Update()
    {
        if (player == null || isFading) return;

        Vector3 dir = player.position - transform.position;
        dir.y = 0;
        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion rot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 2f);
        }

        // Detección del primer avistamiento
        if (!firstSeen)
        {
            Vector3 eyePos = player.position + Vector3.up * 1.6f;
            Vector3 dirToShadow = (transform.position - eyePos).normalized;
            float distance = Vector3.Distance(eyePos, transform.position);
            float angle = Vector3.Angle(player.forward, dirToShadow);

            if (angle < 45f && distance < 20f)
            {
                if (Physics.Raycast(eyePos, dirToShadow, out RaycastHit hit, distance))
                {
                    if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Sombra"))
                    {
                        firstSeen = true;
                        Debug.Log("La sombra ha sido vista por primera vez — Comienza la persecución (RUN).");
                        StartCoroutine(HandleFirstSeenSequence());
                    }
                }
            }
        }
    }

    private IEnumerator HandleFirstSeenSequence()
    {
        canAct = false;
        yield return StartCoroutine(FirstChaseThenDisappear());
        canAct = true;
    }

    private IEnumerator FirstChaseThenDisappear()
    {
        TriggerState(ShadowState.Running);
        agent.isStopped = false;
        agent.speed = 5f;
        agent.SetDestination(player.position);

        recentlyTouched = false;
        StartCoroutine(AutoDisappearIfNotTouched(10f));

        float timer = 0f;
        while (timer < 1.5f)
        {
            timer += Time.deltaTime;
            if (Vector3.Distance(transform.position, player.position) < 2f)
            {
                recentlyTouched = true;
                if (!isFading)
                    StartCoroutine(FadeOutAndReappear());
                yield break;
            }
            yield return null;
        }

        if (!isFading)
            StartCoroutine(FadeOutAndReappear());
    }

    private IEnumerator AutoDisappearIfNotTouched(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        if (!recentlyTouched && !isFading)
        {
            Debug.Log(" La sombra no alcanzó al jugador, desaparece automáticamente...");
            StartCoroutine(FadeOutAndReappear());
        }
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

                int action = Random.Range(0, 4);
                switch (action)
                {
                    case 0: yield return StartCoroutine(StateWithAutoFade(ShadowState.Idle)); break;
                    case 1: yield return StartCoroutine(WalkAroundPlayer()); break;
                    case 2: yield return StartCoroutine(RunTowardsPlayer()); break;
                    case 3: yield return StartCoroutine(StateWithAutoFade(ShadowState.Crying)); break;
                }

                nextActionDelay = Random.Range(minCooldown, maxCooldown);
                Debug.Log($" Próximo ataque estimado en {nextActionDelay:F1} segundos...");
            }
            yield return null;
        }
    }

    // Estados estáticos (Idle o Cry) que desaparecen tras 10 segundos
    private IEnumerator StateWithAutoFade(ShadowState state)
    {
        TriggerState(state);
        yield return new WaitForSeconds(10f);

        if (!isFading)
        {
            Debug.Log($" La sombra estuvo en {state} durante 10s — desaparece para reaparecer cerca...");
            yield return StartCoroutine(FadeOutAndReappear());
        }
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
                if (firstSeen) PlaySound(idleSource, idleClip, 0.7f);
                break;
            case ShadowState.Walking:
                animator.SetTrigger("Walking");
                if (firstSeen) PlaySound(walkSource, walkClip, 0.8f);
                break;
            case ShadowState.Running:
                animator.SetTrigger("Running");
                PlaySound(runSource, runClip, 1f);
                break;
            case ShadowState.Crying:
                animator.SetTrigger("Crying");
                if (firstSeen) PlaySound(crySource, cryClip, 0.9f);
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

        while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
            yield return null;

        TriggerState(ShadowState.Idle);
    }

    private IEnumerator RunTowardsPlayer()
    {
        if (player == null) yield break;
        TriggerState(ShadowState.Running);

        agent.isStopped = false;
        agent.speed = 4f;
        agent.SetDestination(player.position);

        yield return new WaitForSeconds(Random.Range(2f, 4f));
        agent.isStopped = true;
        TriggerState(ShadowState.Idle);
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
        Debug.Log($" La sombra desapareció. Reaparecerá en aproximadamente {delay:F1} segundos...");
        yield return new WaitForSeconds(delay);

        AppearNearPlayer();
        yield return StartCoroutine(Fade(0f, 1f));

        SetRenderersVisible(true);
        if (shadowCollider) shadowCollider.enabled = true;
        isFading = false;

        Debug.Log(" La sombra reapareció cerca del jugador y comenzó una nueva persecución.");
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
