using UnityEngine;

public class Worker : MonoBehaviour
{
    /* This class is the parent class of all worker npcs. */
    public virtual void Delete()
    {
        Destroy(gameObject);
    }
}