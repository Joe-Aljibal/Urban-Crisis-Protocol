using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class StaticResource : MonoBehaviour
{
    [SerializeField] protected float respawnTime = 2.0f;
    public bool IsActiveResource { get; set; } = true;
    public bool IsAvailable { get; set; } = true;

    public void Disable()
    {
        IsActiveResource = false;
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
        IsActiveResource = true;
    }

    private void DisableComponents()
    {
        gameObject.GetComponent<MeshRenderer>().enabled = false;
        gameObject.GetComponent<MeshRenderer>().enabled = false;
        gameObject.GetComponent<NavMeshObstacle>().enabled = false;
    }
}
