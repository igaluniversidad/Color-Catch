using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    private Vector3 initialPosition;
    private Coroutine shakeCoroutine;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        initialPosition = transform.localPosition;
    }

    /// <summary>
    /// Activa una sacudida de cámara con duración y fuerza personalizables.
    /// </summary>
    public void Shake(float duration = 0.08f, float magnitude = 0.12f)
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }
        shakeCoroutine = StartCoroutine(DoShake(duration, magnitude));
    }

    private IEnumerator DoShake(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float offsetX = Random.Range(-1f, 1f) * magnitude;
            float offsetY = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(initialPosition.x + offsetX, initialPosition.y + offsetY, initialPosition.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = initialPosition;
        shakeCoroutine = null;
    }
}