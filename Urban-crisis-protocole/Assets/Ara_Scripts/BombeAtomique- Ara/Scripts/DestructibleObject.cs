using UnityEngine;

public class DestructibleObject : MonoBehaviour, IDestructible
{
    public void DestroyObject()
    {
        Destroy(gameObject);
    }
}
