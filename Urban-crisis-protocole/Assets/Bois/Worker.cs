using UnityEngine;

/// <summary>
/// This class is the parent class of all worker npcs. 
/// </summary>

public class Worker : MonoBehaviour
{
    public virtual void Delete()
    {
        Destroy(gameObject);
    }
}