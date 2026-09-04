using UnityEngine;

public class Worker : MonoBehaviour
{
    public virtual void Delete()
    {
        Destroy(gameObject);
    }
}