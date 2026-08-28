using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Canales de Audio")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    [Header("Clips de Efectos de Sonido (SFX)")]
    [SerializeField] private AudioClip buttonClickClip;
    [SerializeField] private AudioClip ringRotateClip;
    [SerializeField] private AudioClip matchSuccessClip;
    [SerializeField] private AudioClip starMilestoneClip;
    [SerializeField] private AudioClip gameOverClip;
    [SerializeField] private AudioClip purchaseSuccessClip;

    [Header("Música de Fondo (Opcional)")]
    [SerializeField] private AudioClip backgroundMusic;

    private const string VolumeKey = "MasterVolume";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
        if (musicSource == null) musicSource = gameObject.AddComponent<AudioSource>();

        PlayMusic();
    }

    void Start()
    {
        // Carga el volumen guardado (por defecto 1.0 = 100%)
        float savedVolume = PlayerPrefs.GetFloat(VolumeKey, 1f);
        SetMasterVolume(savedVolume);
    }

    public void SetMasterVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);

        AudioListener.volume = volume;
        PlayerPrefs.SetFloat(VolumeKey, volume);
        PlayerPrefs.Save();
    }

    public float GetMasterVolume()
    {
        return PlayerPrefs.GetFloat(VolumeKey, 1f);
    }
    private void PlayMusic()
    {
        if (backgroundMusic != null && musicSource != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.volume = 0.4f;
            musicSource.Play();
        }
    }

    public void PlayButtonClick()
    {
        PlayClip(buttonClickClip, 1f, 0.95f, 1.05f);
    }

    public void PlayRingRotate()
    {
        PlayClip(ringRotateClip, 0.8f, 0.9f, 1.1f);
    }

    public void PlayMatchSuccess()
    {
        // Pequeña variación de pitch para que los aciertos continuos suenen dinámicos
        PlayClip(matchSuccessClip, 1f, 0.95f, 1.15f);
    }

    public void PlayStarMilestone()
    {
        PlayClip(starMilestoneClip, 1f, 1f, 1f);
    }

    public void PlayGameOver()
    {
        PlayClip(gameOverClip, 1f, 1f, 1f);
    }

    public void PlayPurchaseSuccess()
    {
        PlayClip(purchaseSuccessClip, 1f, 1f, 1f);
    }

    private void PlayClip(AudioClip clip, float volume = 1f, float minPitch = 1f, float maxPitch = 1f)
    {
        if (clip == null || sfxSource == null) return;

        sfxSource.pitch = Random.Range(minPitch, maxPitch);
        sfxSource.PlayOneShot(clip, volume);
    }
}