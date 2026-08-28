using TMPro;
using UnityEngine;

public class BlinkingText : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private TextMeshProUGUI targetText;

    [Header("Ajustes de Parpadeo")]
    [Tooltip("Velocidad a la que parpadea el texto")]
    [SerializeField] private float blinkSpeed = 3.5f;

    [Tooltip("Opacidad mínima durante el parpadeo (0 = invisible, 0.2 = casi transparente)")]
    [Range(0f, 1f)]
    [SerializeField] private float minAlpha = 0.15f;

    [Tooltip("Opacidad máxima")]
    [Range(0f, 1f)]
    [SerializeField] private float maxAlpha = 1f;

    private Color originalColor;

    void Awake()
    {
        if (targetText == null)
            targetText = GetComponent<TextMeshProUGUI>();

        if (targetText != null)
            originalColor = targetText.color;
    }

    void OnEnable()
    {
        // Restaura el color inicial cada vez que se enciende el panel
        if (targetText != null)
            targetText.color = originalColor;
    }

    void Update()
    {
        if (targetText == null) return;

        // Oscilación suave entre minAlpha y maxAlpha
        float t = (Mathf.Sin(Time.time * blinkSpeed) + 1f) * 0.5f;
        float currentAlpha = Mathf.Lerp(minAlpha, maxAlpha, t);

        Color c = originalColor;
        c.a = currentAlpha;
        targetText.color = c;
    }

    void OnDisable()
    {
        if (targetText != null)
            targetText.color = originalColor;
    }
}