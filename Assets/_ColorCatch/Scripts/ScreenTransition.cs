using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenTransition : MonoBehaviour
{
    public static ScreenTransition Instance { get; private set; }

    [Header("Referencias")]
    [SerializeField] private RectTransform wipeShape;
    [SerializeField] private Image wipeImage;

    [Header("Configuración")]
    [Tooltip("Escala máxima para cubrir toda la pantalla vertical")]
    [SerializeField] private float targetScale = 35f;
    [SerializeField] private float defaultDuration = 0.35f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (wipeShape == null)
            wipeShape = GetComponent<RectTransform>();

        if (wipeImage == null)
            wipeImage = GetComponent<Image>();

        // Inicia completamente abierto
        wipeShape.localScale = Vector3.zero;
        if (wipeImage != null) wipeImage.raycastTarget = false;
    }

    /// <summary>
    /// Cierra la pantalla con la forma negra, ejecuta la acción (cambio de panel), y vuelve a abrirse.
    /// </summary>
    public void PlayTransition(Action onScreenCovered, float duration = -1f)
    {
        float speed = (duration > 0f) ? duration : defaultDuration;
        StartCoroutine(TransitionRoutine(onScreenCovered, speed));
    }

    private IEnumerator TransitionRoutine(Action onScreenCovered, float duration)
    {
        if (wipeImage != null) wipeImage.raycastTarget = true;

        // 1. CERRAR PANTALLA (Usa unscaledDeltaTime para funcionar con Time.timeScale = 0)
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            float ease = t * t * t;
            float currentScale = Mathf.Lerp(0f, targetScale, ease);
            wipeShape.localScale = new Vector3(currentScale, currentScale, 1f);
            yield return null;
        }
        wipeShape.localScale = new Vector3(targetScale, targetScale, 1f);

        // 2. ACCIÓN (Cambio de panel)
        onScreenCovered?.Invoke();
        yield return new WaitForSecondsRealtime(0.05f);

        // 3. ABRIR PANTALLA
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            float ease = 1f - Mathf.Pow(1f - t, 3);
            float currentScale = Mathf.Lerp(targetScale, 0f, ease);
            wipeShape.localScale = new Vector3(currentScale, currentScale, 1f);
            yield return null;
        }

        wipeShape.localScale = Vector3.zero;
        if (wipeImage != null) wipeImage.raycastTarget = false;
    }
}