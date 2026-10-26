using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Menu pause de MainGame : Echap ouvre/ferme le menu et met le jeu en pause.
public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel; // MenuBreak
    [SerializeField] private string homeSceneName = "HomeScreen";

    bool isPaused = false;

    void Start()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused) Resume();
            else Pause();
        }
    }

    public void Pause()
    {
        isPaused = true;
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    // Bouton Resume
    public void Resume()
    {
        isPaused = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    // Bouton Quit : retour a l'ecran d'accueil
    public void QuitToHome()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(homeSceneName);
    }
}
