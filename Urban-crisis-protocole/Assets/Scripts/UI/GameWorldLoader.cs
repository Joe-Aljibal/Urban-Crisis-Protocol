using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// A mettre dans MainGame : charge la scene du jeu (grille, construction) par-dessus l'UI.
// Ainsi l'UI reste dans MainGame et le jeu reste dans la scene de l'equipe.
public class GameWorldLoader : MonoBehaviour
{
    // Nom de la scene du jeu. Elle doit etre dans File > Build Profiles > Scene List.
    [SerializeField] private string gameSceneName = "SampleScene";

    private IEnumerator Start()
    {
        if (SceneManager.GetSceneByName(gameSceneName).isLoaded)
            yield break;

        yield return SceneManager.LoadSceneAsync(gameSceneName, LoadSceneMode.Additive);

        // La scene du jeu devient la scene active : sa lumiere et son ciel sont utilises,
        // et les batiments construits y sont crees.
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(gameSceneName));
    }
}
