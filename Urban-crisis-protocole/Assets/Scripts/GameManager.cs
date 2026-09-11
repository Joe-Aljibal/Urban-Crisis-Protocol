using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Singleton instance of the GameManager class.
    // This ensures that there is only one instance of GameManager throughout the game, which can be accessed globally.
    static GameManager instance; 
                                 


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        // DontDestroyOnLoad(gameObject) is used to make sure that the GameManager object persists across scene loads.
        // This is important for maintaining game state, player progress, and other data that should not be reset when changing scenes.
        DontDestroyOnLoad(gameObject);
        // Check if an instance of GameManager already exists. If it does and it's not this instance, destroy this game object to enforce the singleton pattern.
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else // If no instance exists, set this as the instance of GameManager.
        {
            instance = this;
        }
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        CheckInput(); // Call the CheckInput method to handle user input for scene transitions.
    }



    // LoadChosenScene is a method that loads the specified scene in the game.
    void LoadChosenScene(string NameOfTheScene)
    {
        SceneManager.LoadScene(NameOfTheScene);
    }


    void CheckInput()
    {
        string scene = SceneManager.GetActiveScene().name; // Get the name of the currently active scene and store it in the variable 'scene'.
        if (scene == "HomeScreen") // Check if the current scene is "MainMenu".
        {
            if (Keyboard.current.enterKey.IsPressed()) // If the Enter key on the keypad is pressed, load the "MainGame" scene.
            {

                LoadChosenScene("MainGame");
            }
        }
        else if (scene == "MainGame") // Check if the current scene is "MainGame".
        {
            if (Keyboard.current.escapeKey.IsPressed()) // If the Escape key is pressed, load the "MainMenu" scene.
            {
                LoadChosenScene("Main Menu");
            }
        } 
    }
}
