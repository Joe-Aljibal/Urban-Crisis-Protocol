using System.Collections;
using UnityEngine;

public class ViewModelCam : MonoBehaviour
{
    public GameObject model;
    public float speed = 1f;
    public float height = 0f;

    Vector3 initialPosition;
    Coroutine spinningCoroutine;

    void Start()
    {
        initialPosition = transform.position;
    }

    public void StartSpinning()
    {
        StopSpinning();
        spinningCoroutine = StartCoroutine(SpinningCoroutine());
    }

    public void StopSpinning()
    {
        if (spinningCoroutine != null)
        {
            StopCoroutine(spinningCoroutine);
            spinningCoroutine = null;
        }
        transform.position = initialPosition;
        transform.LookAt(model.transform.position);
    }

    IEnumerator SpinningCoroutine()
    {
        transform.position = initialPosition;

        Vector3 center = model.transform.position;

        Vector3 offset = initialPosition - center;
        float radius = new Vector2(offset.x, offset.z).magnitude;
        float angle = Mathf.Atan2(offset.z, offset.x);
        float y = offset.y + height;

        while (true)
        {
            angle += speed * Time.deltaTime;

            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;

            transform.position = center + new Vector3(x, y, z);
            transform.LookAt(center);
            yield return null;
        }
    }
}