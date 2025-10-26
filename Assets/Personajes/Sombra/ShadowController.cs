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

    [Header("Comportamiento aleatorio")]
    public float minActionTime = 3f;
    public float maxActionTime = 7f;
    public float detectionRadius = 10f;
    public float appearDistanceMin = 6f;
    public float appearDistanceMax = 12f;

    [Header("Desaparición y aparición")]
    public float fadeDuration = 2f;
    public float reappearDelayMin = 10f;
    public float reappearDelayMax = 30f;
    private SkinnedMeshRenderer[] renderers;
    private bool isFading = false;

    [Header("Intervalo entre comportamientos")]
    public float minCooldown = 15f;
    public float maxCooldown = 60f;

    private bool firstSeen = false;
    private bool canAct = false;
    private float nextActionDelay = 0f;

    private enum ShadowState { Idle, Walking, Running, Crying }
    private ShadowState currentState;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        renderers = GetComponentsInChildren<SkinnedMeshRenderer>();

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        canAct = false;
        StartCoroutine(RandomBehaviorLoop());
    }

    private void Update()
    {
        if (player == null || isFading) return;

        // Siempre mira hacia el jugador
        Vector3 dir = player.position - transform.position;
        dir.y = 0;
        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion rot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 2f);
        }

        // Detectar si el jugador realmente puede ver a la sombra
        if (!firstSeen)
        {
            Vector3 eyePos = player.position + Vector3.up * 1.6f; // altura de los ojos
            Vector3 dirToShadow = (transform.position - eyePos).normalized;
            float distance = Vector3.Distance(eyePos, transform.position);
            float angle = Vector3.Angle(player.forward, dirToShadow);

            // Solo dentro del campo visual y a distancia razonable
            if (angle < 45f && distance < 20f)
            {
                // Raycast: confirma que no haya obstáculos y que golpee a la sombra
                if (Physics.Raycast(eyePos, dirToShadow, out RaycastHit hit, distance))
                {
                    if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Sombra"))
                    {
                        firstSeen = true;
                        canAct = true;
                        Debug.Log("La sombra ha sido vista por primera vez — corre hacia el jugador");
                        StartCoroutine(FirstChaseThenDisappear());
                    }
                }
            }
        }
    }




    private void OnBecameVisible()
    {
        if (!firstSeen)
        {
            firstSeen = true;
            canAct = true;
            Debug.Log("La sombra ha sido vista por primera vez");
            TriggerState(ShadowState.Walking);
            StartCoroutine(DisappearAfterSeen());
        }
    }

    private IEnumerator DisappearAfterSeen()
    {
        yield return new WaitForSeconds(0.8f);
        if (!isFading)
            StartCoroutine(FadeOutAndReappear());
    }
    private IEnumerator FirstChaseThenDisappear()
    {
        // Primera reacción: correr hacia el jugador
        TriggerState(ShadowState.Running);
        agent.isStopped = false;
        agent.speed = 5f;
        agent.SetDestination(player.position);

        yield return new WaitForSeconds(1.2f); // corre brevemente hacia él

        // Luego desaparece
        if (!isFading)
            StartCoroutine(FadeOutAndReappear());
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
                    case 0: TriggerState(ShadowState.Idle); break;
                    case 1: yield return StartCoroutine(WalkAroundPlayer()); break;
                    case 2: yield return StartCoroutine(RunTowardsPlayer()); break;
                    case 3: TriggerState(ShadowState.Crying); break;
                }

                nextActionDelay = Random.Range(minCooldown, maxCooldown);
            }

            yield return null;
        }
    }

    private void TriggerState(ShadowState state)
    {
        currentState = state;
        ResetTriggers();

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

    private void ResetTriggers()
    {
        animator.ResetTrigger("Idle");
        animator.ResetTrigger("Walking");
        animator.ResetTrigger("Running");
        animator.ResetTrigger("Crying");
        StopAllAudio();
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

    public void AppearNearPlayer()
    {
        if (player == null) return;

        Vector3 randomDir = player.forward * Random.Range(appearDistanceMin, appearDistanceMax);
        randomDir += new Vector3(Random.Range(-3f, 3f), 0, Random.Range(-3f, 3f));
        Vector3 spawnPos = player.position + randomDir;

        if (NavMesh.SamplePosition(spawnPos, out NavMeshHit hit, appearDistanceMax, NavMesh.AllAreas))
        {
            transform.position = hit.position;
            StartCoroutine(FadeIn());
        }
    }

    public IEnumerator FadeOutAndReappear()
    {
        if (isFading) yield break;
        isFading = true;

        yield return StartCoroutine(Fade(1f, 0f));
        agent.isStopped = true;
        gameObject.SetActive(false);

        yield return new WaitForSeconds(Random.Range(reappearDelayMin, reappearDelayMax));

        gameObject.SetActive(true);
        AppearNearPlayer();
        yield return StartCoroutine(Fade(0f, 1f));
        isFading = false;
        Debug.Log("La sombra reaparece y comienza persecución");
    }

    private IEnumerator FadeIn()
    {
        yield return StartCoroutine(Fade(0f, 1f));
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
