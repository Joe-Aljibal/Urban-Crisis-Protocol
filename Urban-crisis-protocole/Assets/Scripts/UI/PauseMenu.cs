using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Menu pause de MainGame : Echap ouvre/ferme le menu et met le jeu en pause.
// Le bouton Setting ouvre le panneau des parametres ; Echap ou Back y revient.
public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;    // MenuBreak
    [SerializeField] private GameObject settingsPanel; // SettingsPanel
    [SerializeField] private string homeSceneName = "HomeScreen";

    bool isPaused = false;

    // UI cachee pendant la pause (HUD, canvas du jeu), reaffichee a la reprise.
    readonly List<GameObject> hiddenObjects = new();
    readonly List<Canvas> hiddenCanvases = new();

    void Start()
    {
        pausePanel.SetActive(false);
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    void Update()
    {
        if (Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame)
            return;

        if (settingsPanel != null && settingsPanel.activeSelf) CloseSettings();
        else if (isPaused) Resume();
        else Pause();
    }

    public void Pause()
    {
        isPaused = true;
        HideOtherUI();
        pausePanel.SetActive(true);
        SetGamePaused(true);
    }

    // Bouton Resume
    public void Resume()
    {
        isPaused = false;
        pausePanel.SetActive(false);
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
        ShowOtherUI();
        SetGamePaused(false);
    }

    // Cache tout sauf le menu : les voisins de MenuBreak (HUD) et les autres canvas du jeu.
    void HideOtherUI()
    {
        foreach (Transform sibling in pausePanel.transform.parent)
        {
            GameObject go = sibling.gameObject;
            if (go == pausePanel || go == settingsPanel || !go.activeSelf)
                continue;

            go.SetActive(false);
            hiddenObjects.Add(go);
        }

        Canvas menuCanvas = pausePanel.GetComponentInParent<Canvas>(true).rootCanvas;
        foreach (Canvas canvas in FindObjectsByType<Canvas>(FindObjectsSortMode.None))
        {
            if (canvas == menuCanvas || !canvas.isRootCanvas || !canvas.enabled)
                continue;

            canvas.enabled = false;
            hiddenCanvases.Add(canvas);
        }
    }

    void ShowOtherUI()
    {
        foreach (GameObject go in hiddenObjects)
            if (go != null) go.SetActive(true);

        foreach (Canvas canvas in hiddenCanvases)
            if (canvas != null) canvas.enabled = true;

        hiddenObjects.Clear();
        hiddenCanvases.Clear();
    }

    // Bouton Setting
    public void OpenSettings()
    {
        if (settingsPanel == null)
            return;

        pausePanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    // Bouton Back du panneau des parametres
    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        pausePanel.SetActive(true);
    }

    // Bouton Quit : retour a l'ecran d'accueil
    public void QuitToHome()
    {
        SetGamePaused(false);
        SceneManager.LoadScene(homeSceneName);
    }

    // Si la scene se ferme pendant la pause, le jeu ne reste pas fige.
    void OnDestroy()
    {
        if (isPaused)
            SetGamePaused(false);
    }

    // Arrete le temps et les effets sonores ; la musique continue, plus bas.
    static void SetGamePaused(bool paused)
    {
        Time.timeScale = paused ? 0f : 1f;
        AudioListener.pause = paused;
        MusicManager.SetPaused(paused);
    }
}
