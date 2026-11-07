using System;
using UnityEngine;

// Volumes du jeu, sauvegardes entre les parties (PlayerPrefs).
// General : tout le son du jeu (AudioListener). Musique : seulement le MusicManager.
public static class GameAudioSettings
{
    const string MasterKey = "audio.masterVolume";
    const string MusicKey = "audio.musicVolume";

    public static event Action Changed;

    public static float MasterVolume
    {
        get => PlayerPrefs.GetFloat(MasterKey, 1f);
        set
        {
            PlayerPrefs.SetFloat(MasterKey, Mathf.Clamp01(value));
            Apply();
        }
    }

    public static float MusicVolume
    {
        get => PlayerPrefs.GetFloat(MusicKey, 0.6f);
        set
        {
            PlayerPrefs.SetFloat(MusicKey, Mathf.Clamp01(value));
            Apply();
        }
    }

    public static void Save() => PlayerPrefs.Save();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Apply()
    {
        AudioListener.volume = MasterVolume;
        Changed?.Invoke();
    }
}
