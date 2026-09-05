using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class TreeScript : MonoBehaviour
{
    [SerializeField] private float respawnTime = 2.0f;
    public bool IsActiveRessource { get; set; } = true;
    public bool IsAvailable { get; set; } = true;

    public void DisableTree()
    {
        IsActiveRessource = false;
        DisableComponents();
        StartCoroutine(RespawnCoroutine());
    }

    private IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(respawnTime);
        ActivateComponents();
    }

    private void ActivateComponents()
    {
        gameObject.GetComponent<MeshRenderer>().enabled = true;
        gameObject.GetComponent<MeshRenderer>().enabled = true;
        gameObject.GetComponent<NavMeshObstacle>().enabled = true;
        IsActiveRessource = true;
    }

    private void DisableComponents()
    {
        gameObject.GetComponent<MeshRenderer>().enabled = false;
        gameObject.GetComponent<MeshRenderer>().enabled = false;
        gameObject.GetComponent<NavMeshObstacle>().enabled = false;
    }
}
