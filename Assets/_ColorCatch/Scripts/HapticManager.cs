using UnityEngine;

public class HapticManager : MonoBehaviour
{
    public static HapticManager Instance { get; private set; }

    [Header("Configuración")]
    [SerializeField] private bool hapticsEnabled = true;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Vibración sutil para giros de rueda o clics de botones (40 ms).
    /// </summary>
    public void TriggerLightFeedback()
    {
        Vibrate(40);
    }

    /// <summary>
    /// Vibración media para aciertos de orbe (80 ms).
    /// </summary>
    public void TriggerSuccessFeedback()
    {
        Vibrate(80);
    }

    /// <summary>
    /// Vibración intensa al alcanzar 10 puntos y ganar estrella (150 ms).
    /// </summary>
    public void TriggerMilestoneFeedback()
    {
        Vibrate(150);
    }

    /// <summary>
    /// Vibración fuerte para Game Over (300 ms).
    /// </summary>
    public void TriggerFailureFeedback()
    {
        Vibrate(300);
    }

    public void ToggleHaptics(bool enabled)
    {
        hapticsEnabled = enabled;
    }

    private void Vibrate(long milliseconds)
    {
        if (!hapticsEnabled) return;

#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                AndroidJavaObject vibrator = null;

                using (AndroidJavaClass versionClass = new AndroidJavaClass("android.os.Build$VERSION"))
                {
                    int sdkInt = versionClass.GetStatic<int>("SDK_INT");
                    if (sdkInt >= 31) // Android 12+
                    {
                        using (AndroidJavaObject vibratorManager = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator_manager"))
                        {
                            if (vibratorManager != null)
                            {
                                vibrator = vibratorManager.Call<AndroidJavaObject>("getDefaultVibrator");
                            }
                        }
                    }
                }

                if (vibrator == null)
                {
                    vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
                }

                if (vibrator != null && vibrator.Call<bool>("hasVibrator"))
                {
                    using (AndroidJavaClass vibrationEffectClass = new AndroidJavaClass("android.os.VibrationEffect"))
                    {
                        AndroidJavaObject effect = vibrationEffectClass.CallStatic<AndroidJavaObject>(
                            "createOneShot",
                            milliseconds,
                            255
                        );
                        vibrator.Call("vibrate", effect);
                    }
                    vibrator.Dispose();
                    return;
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("[Haptics] Fallback nativo: " + e.Message);
        }

        Handheld.Vibrate();
#elif UNITY_IOS && !UNITY_EDITOR
        Handheld.Vibrate();
#endif
    }
}