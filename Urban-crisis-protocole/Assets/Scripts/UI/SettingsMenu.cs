using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Panneau Settings du menu pause : sliders de volume (0 a 100 %), sauvegardes a la fermeture.
public class SettingsMenu : MonoBehaviour
{
    [SerializeField] private Slider masterSlider;
    [SerializeField] private TMP_Text masterValue;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private TMP_Text musicValue;

    void Awake()
    {
        masterSlider.onValueChanged.AddListener(value =>
        {
            GameAudioSettings.MasterVolume = value;
            ShowPercent(masterValue, value);
        });

        musicSlider.onValueChanged.AddListener(value =>
        {
            GameAudioSettings.MusicVolume = value;
            ShowPercent(musicValue, value);
        });
    }

    void OnEnable()
    {
        masterSlider.SetValueWithoutNotify(GameAudioSettings.MasterVolume);
        musicSlider.SetValueWithoutNotify(GameAudioSettings.MusicVolume);
        ShowPercent(masterValue, masterSlider.value);
        ShowPercent(musicValue, musicSlider.value);
    }

    void OnDisable() => GameAudioSettings.Save();

    static void ShowPercent(TMP_Text label, float value)
    {
        if (label != null)
            label.text = Mathf.RoundToInt(value * 100) + " %";
    }
}
