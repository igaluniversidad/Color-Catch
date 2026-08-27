using UnityEngine;

public class PulseAnimation : MonoBehaviour
{
    [Header("Configuración del Pulso")]
    [Tooltip("Escala mínima a la que se encoge el botón")]
    [SerializeField] private float minScale = 0.92f;

    [Tooltip("Escala máxima a la que se agranda el botón")]
    [SerializeField] private float maxScale = 1.08f;

    [Tooltip("Velocidad de la animación")]
    [SerializeField] private float pulseSpeed = 4f;

    private Vector3 initialScale;

    void Awake()
    {
        initialScale = transform.localScale;
    }

    void OnEnable()
    {
        // Restablece la escala original al encenderse el panel
        transform.localScale = initialScale;
    }

    void Update()
    {
        // Oscilación suave entre 0 y 1
        float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;

        // Suavizado cúbico para que la transición sea más orgánica (Ease In-Out)
        float smoothT = Mathf.SmoothStep(0f, 1f, t);

        float currentMultiplier = Mathf.Lerp(minScale, maxScale, smoothT);
        transform.localScale = initialScale * currentMultiplier;
    }

    void OnDisable()
    {
        transform.localScale = initialScale;
    }
}