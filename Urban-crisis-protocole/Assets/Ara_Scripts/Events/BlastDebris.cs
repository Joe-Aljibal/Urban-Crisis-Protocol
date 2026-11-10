using UnityEngine;

// Keep debris physics bounded even when many buildings are destroyed together.
public class BlastDebris : MonoBehaviour
{
    private static int activePieces;
    private bool counted;

    public static void Spawn(Renderer source)
    {
        if (source == null) return;
        Bounds bounds = source.bounds;
        for (int i = 0; i < 6 && activePieces < 64; i++)
        {
            GameObject piece = GameObject.CreatePrimitive(PrimitiveType.Cube);
            piece.name = "Blast debris";
            piece.transform.position = bounds.center + Random.insideUnitSphere * 0.8f;
            piece.transform.rotation = Random.rotation;
            piece.transform.localScale = Vector3.Min(bounds.size * 0.2f, Vector3.one * 1.5f);
            piece.GetComponent<Renderer>().sharedMaterial = source.sharedMaterial;
            Rigidbody body = piece.AddComponent<Rigidbody>();
            body.mass = 2f;
            body.AddExplosionForce(90f, bounds.center - Vector3.up * 2f, 10f, 2f, ForceMode.Impulse);
            BlastDebris lifetime = piece.AddComponent<BlastDebris>();
            lifetime.counted = true;
            activePieces++;
            Destroy(piece, 7f);
        }
    }

    private void OnDestroy() { if (counted) activePieces--; }
}