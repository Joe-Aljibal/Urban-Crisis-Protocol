using UnityEngine;

// Musique de fond : joue en boucle les morceaux de Resources/Music, avec un fondu entre chaque.
// Se cree tout seul au lancement et reste d'une scene a l'autre : rien a placer dans les scenes.
[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    const string MusicFolder = "Music"; // Assets/Audio/Resources/Music

    [SerializeField] bool shuffle = true;
    [SerializeField] float fadeDuration = 2f;
    [SerializeField, Range(0, 1)] float pausedVolume = 0.35f; // musique plus basse en pause

    static MusicManager instance;

    AudioClip[] playlist;
    AudioSource source;
    int current = -1;
    bool paused;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void CreateOnStartup()
    {
        if (instance != null)
            return;

        new GameObject("MusicManager", typeof(AudioSource), typeof(MusicManager));
    }

    // Le menu pause baisse la musique sans la couper.
    public static void SetPaused(bool isPaused)
    {
        if (instance == null)
            return;

        instance.paused = isPaused;
        instance.UpdateVolume();
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        source = GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0;           // son 2D, pareil partout dans la ville
        source.ignoreListenerPause = true; // continue quand le menu pause coupe les effets

        playlist = Resources.LoadAll<AudioClip>(MusicFolder);
        if (playlist.Length == 0)
        {
            Debug.LogWarning("[Musique] Aucun morceau dans Assets/Audio/Resources/Music.");
            enabled = false;
            return;
        }

        GameAudioSettings.Changed += UpdateVolume;
        PlayNext();
    }

    void OnDestroy()
    {
        if (instance != this)
            return;

        GameAudioSettings.Changed -= UpdateVolume;
        instance = null;
    }

    void Update()
    {
        if (!source.isPlaying && Application.isFocused)
            PlayNext();

        UpdateVolume();
    }

    void PlayNext()
    {
        if (shuffle && current < 0)
        {
            current = Random.Range(0, playlist.Length);
        }
        else if (shuffle && playlist.Length > 1)
        {
            int next = Random.Range(0, playlist.Length - 1);
            current = next >= current ? next + 1 : next; // jamais deux fois le meme morceau
        }
        else
        {
            current = (current + 1) % playlist.Length;
        }

        source.clip = playlist[current];
        source.Play();
    }

    void UpdateVolume()
    {
        if (source == null || source.clip == null)
            return;

        // Fondu au debut et a la fin du morceau.
        float remaining = source.clip.length - source.time;
        float fade = fadeDuration > 0 ? Mathf.Clamp01(Mathf.Min(source.time, remaining) / fadeDuration) : 1;

        source.volume = GameAudioSettings.MusicVolume * fade * (paused ? pausedVolume : 1);
    }
}
