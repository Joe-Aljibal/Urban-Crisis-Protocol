using System.Collections;
using UnityEngine;

public class GameEndManager : MonoBehaviour
{
    [SerializeField]
    private BombDetonationController detonationController;

    [SerializeField]
    private GameObject gameEndPanel;

    [SerializeField]
    private float gameEndDelay = 5f;
    private bool gameHasEnded = false;
    private Coroutine gameFinished;

    private void Start()
    {
        Time.timeScale = 1f;

        // gameEndPanel.SetActive(false);
        detonationController.OnDetonated += HandleBombDetonated;
    }

    private void HandleBombDetonated(Vector3 explosionPosiotion)
    {
        if (gameHasEnded)
        {
            return;
        }
        gameHasEnded = true;
        gameFinished = StartCoroutine(EndGameSequence());
    }

    private IEnumerator EndGameSequence()
    {
        yield return new WaitForSeconds(gameEndDelay);
        if (gameEndPanel != null)
        {
            gameEndPanel.SetActive(true);
        }
        Debug.Log("the game has ended");
        Time.timeScale = 0f;
    }

    private void OnDestroy()
    {
        detonationController.OnDetonated -= HandleBombDetonated;
    }
}
