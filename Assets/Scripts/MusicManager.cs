using UnityEngine;

public class MusicManager : MonoBehaviour
{
    private static MusicManager instancia;
    private AudioSource audioSource;
    private static float volumenOriginal = 1f;
    private static bool enPausa = false;

    void Awake()
    {
        // Singleton (solo una instancia viva)
        if (instancia != null && instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        instancia = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.loop = true;
            audioSource.Play();
        }

        volumenOriginal = AudioListener.volume;
    }

    // ---------------------------------------------------------------------
    // CONTROL DE MÚSICA INDIVIDUAL
    // ---------------------------------------------------------------------

    public static void PausarMusica()
    {
        if (instancia != null && instancia.audioSource != null && instancia.audioSource.isPlaying)
            instancia.audioSource.Pause();
    }

    public static void ReanudarMusica()
    {
        if (instancia != null && instancia.audioSource != null)
            instancia.audioSource.UnPause();
    }

    public static void DetenerMusica()
    {
        if (instancia != null && instancia.audioSource != null)
            instancia.audioSource.Stop();
    }

    // ---------------------------------------------------------------------
    // CONTROL GLOBAL DE VOLUMEN
    // ---------------------------------------------------------------------

    public static void SilenciarTodo()
    {
        enPausa = true;
        volumenOriginal = AudioListener.volume;
        AudioListener.volume = 0f; // silencia absolutamente todo
        Debug.Log("[MusicManager] Todo el audio silenciado (pausa).");
    }

    public static void RestaurarVolumen()
    {
        if (!enPausa) return;
        enPausa = false;
        AudioListener.volume = volumenOriginal;
        Debug.Log("[MusicManager] Volumen restaurado.");
    }
}
