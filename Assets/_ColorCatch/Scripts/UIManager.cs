using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameManager;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Paneles Principales")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;

    [Header("Menú Principal")]
    [SerializeField] private TextMeshProUGUI menuHighScoreText;
    [SerializeField] private TextMeshProUGUI menuStarsText;
    [SerializeField] private Button playButton;
    [SerializeField] private Button openShopButton;

    [Header("Tienda")]
    [SerializeField] private TextMeshProUGUI shopStarsText;
    [SerializeField] private Button closeShopButton;

    [Header("HUD (En Juego)")]
    [SerializeField] private TextMeshProUGUI gameplayScoreText;
    [SerializeField] private GameObject scoreParticlePrefab;
    [SerializeField] private Button pauseButton;

    [Header("Panel Pausa")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button pauseShopButton;
    [SerializeField] private Button pauseMenuButton;

    [Header("Panel Game Over")]
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI bestScoreText;
    [SerializeField] private TextMeshProUGUI earnedStarsText;
    [SerializeField] private Button restartButton;

    [Header("Ajustes / Audio")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Image volumeIcon;
    [SerializeField] private Sprite soundOnSprite;
    [SerializeField] private Sprite soundMuteSprite;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Registro de botones con SFX y Haptics
        RegisterButton(playButton, OnPlayClicked);
        RegisterButton(openShopButton, OnOpenShopFromMenuClicked);
        RegisterButton(closeShopButton, OnCloseShopClicked);
        RegisterButton(pauseButton, OnPauseClicked);
        RegisterButton(resumeButton, OnResumeClicked);
        RegisterButton(pauseShopButton, OnOpenShopFromPauseClicked);
        RegisterButton(pauseMenuButton, OnPauseMenuClicked);
        RegisterButton(restartButton, OnRestartClicked);
    }

    void OnEnable()
    {
        GameManager.OnEnterWaitingToStart += ShowTutorialScreen;
        GameManager.OnGameStarted += HideTutorialAndShowHUD;
        GameManager.OnGamePaused += ShowPauseScreen;
        GameManager.OnGameResumed += HidePauseScreen;
        GameManager.OnScoreUpdated += UpdateScoreHUD;
        GameManager.OnGameOver += ShowGameOverScreen;
        GameManager.OnStarsUpdated += UpdateStarsUI;
    }

    void OnDisable()
    {
        GameManager.OnEnterWaitingToStart -= ShowTutorialScreen;
        GameManager.OnGameStarted -= HideTutorialAndShowHUD;
        GameManager.OnGamePaused -= ShowPauseScreen;
        GameManager.OnGameResumed -= HidePauseScreen;
        GameManager.OnScoreUpdated -= UpdateScoreHUD;
        GameManager.OnGameOver -= ShowGameOverScreen;
        GameManager.OnStarsUpdated -= UpdateStarsUI;
    }

    void Start()
    {
        SetPanelImmediate(mainMenuPanel, true);
        SetPanelImmediate(shopPanel, false);
        SetPanelImmediate(tutorialPanel, false);
        SetPanelImmediate(hudPanel, false);
        SetPanelImmediate(pausePanel, false);
        SetPanelImmediate(gameOverPanel, false);

        UpdateMenuData();
        if (GameManager.Instance != null)
        {
            UpdateStarsUI(GameManager.Instance.TotalStars);
        }

        InitVolumeSlider();
    }

    void Update()
    {
        bool backPressed = false;

#if ENABLE_INPUT_SYSTEM
        if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            backPressed = true;
        }
#else
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            backPressed = true;
        }
#endif

        if (backPressed)
        {
            HandleBackButton();
        }
    }

    private void InitVolumeSlider()
    {
        if (volumeSlider != null)
        {
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;

            float initialVolume = AudioManager.Instance != null
                ? AudioManager.Instance.GetMasterVolume()
                : PlayerPrefs.GetFloat("MasterVolume", 1f);

            volumeSlider.value = initialVolume;
            UpdateVolumeIcon(initialVolume);

            volumeSlider.onValueChanged.RemoveAllListeners();
            volumeSlider.onValueChanged.AddListener((val) =>
            {
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.SetMasterVolume(val);
                }
                else
                {
                    AudioListener.volume = Mathf.Clamp01(val);
                    PlayerPrefs.SetFloat("MasterVolume", val);
                    PlayerPrefs.Save();
                }

                UpdateVolumeIcon(val);
            });
        }
    }

    private void UpdateVolumeIcon(float volume)
    {
        if (volumeIcon == null) return;

        if (volume <= 0.01f)
        {
            if (soundMuteSprite != null) volumeIcon.sprite = soundMuteSprite;
        }
        else
        {
            if (soundOnSprite != null) volumeIcon.sprite = soundOnSprite;
        }
    }

    private void HandleBackButton()
    {
        if (shopPanel != null && shopPanel.activeSelf)
        {
            AudioManager.Instance?.PlayButtonClick();
            HapticManager.Instance?.TriggerLightFeedback();
            OnCloseShopClicked();
            return;
        }

        if (pausePanel != null && pausePanel.activeSelf)
        {
            AudioManager.Instance?.PlayButtonClick();
            HapticManager.Instance?.TriggerLightFeedback();
            OnResumeClicked();
            return;
        }

        if (GameManager.Instance != null &&
           (GameManager.Instance.CurrentState == GameState.Playing || GameManager.Instance.CurrentState == GameState.WaitingToStart))
        {
            AudioManager.Instance?.PlayButtonClick();
            HapticManager.Instance?.TriggerLightFeedback();
            OnPauseClicked();
            return;
        }

        if (gameOverPanel != null && gameOverPanel.activeSelf)
        {
            AudioManager.Instance?.PlayButtonClick();
            HapticManager.Instance?.TriggerLightFeedback();
            OnRestartClicked();
            return;
        }

        if (mainMenuPanel != null && mainMenuPanel.activeSelf)
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }

    private void RegisterButton(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null) return;
        button.onClick.AddListener(() =>
        {
            AudioManager.Instance?.PlayButtonClick();
            HapticManager.Instance?.TriggerLightFeedback();
            action?.Invoke();
        });
    }

    public void UpdateMenuData()
    {
        if (GameManager.Instance != null)
        {
            if (menuHighScoreText != null) menuHighScoreText.text = $"BEST: {GameManager.Instance.HighScore}";
            UpdateStarsUI(GameManager.Instance.TotalStars);
        }
    }

    public void UpdateStarsUI(int totalStars)
    {
        if (menuStarsText != null) menuStarsText.text = totalStars.ToString();
        if (shopStarsText != null) shopStarsText.text = totalStars.ToString();
    }

    private void OnPlayClicked()
    {
        ScreenTransition.Instance.PlayTransition(() =>
        {
            SetPanelImmediate(mainMenuPanel, false);
            GameManager.Instance?.PrepareGame();
        });
    }

    private void OnOpenShopFromMenuClicked()
    {
        ScreenTransition.Instance.PlayTransition(() =>
        {
            SetPanelImmediate(mainMenuPanel, false);
            SetPanelImmediate(shopPanel, true);
        });
    }

    private void OnOpenShopFromPauseClicked()
    {
        SetPanelImmediate(pausePanel, false);
        SetPanelImmediate(hudPanel, false);

        GameManager.Instance?.RestartGame();
        Time.timeScale = 1f;

        ScreenTransition.Instance.PlayTransition(() =>
        {
            SetPanelImmediate(shopPanel, true);
        });
    }

    private void OnCloseShopClicked()
    {
        ScreenTransition.Instance.PlayTransition(() =>
        {
            SetPanelImmediate(shopPanel, false);
            SetPanelImmediate(mainMenuPanel, true);
            UpdateMenuData();
        });
    }

    private void ShowTutorialScreen()
    {
        SetPanelImmediate(tutorialPanel, true);
        SetPanelImmediate(hudPanel, true);
    }

    private void HideTutorialAndShowHUD()
    {
        SetPanelImmediate(tutorialPanel, false);
    }

    private void OnPauseClicked()
    {
        GameManager.Instance?.PauseGame();
    }

    private void ShowPauseScreen()
    {
        SetPanelImmediate(pausePanel, true);
    }

    private void OnResumeClicked()
    {
        GameManager.Instance?.ResumeGame();
    }

    private void HidePauseScreen()
    {
        SetPanelImmediate(pausePanel, false);
    }

    private void OnPauseMenuClicked()
    {
        SetPanelImmediate(pausePanel, false);
        GameManager.Instance?.RestartGame();
        Time.timeScale = 1f;

        ScreenTransition.Instance.PlayTransition(() =>
        {
            SetPanelImmediate(hudPanel, false);
            SetPanelImmediate(mainMenuPanel, true);
            UpdateMenuData();
        });
    }

    private void ShowGameOverScreen(int finalScore, int highScore, int earnedStars)
    {
        if (finalScoreText != null) finalScoreText.text = $"SCORE: {finalScore}";
        if (bestScoreText != null) bestScoreText.text = $"BEST: {highScore}";
        if (earnedStarsText != null) earnedStarsText.text = $"+{earnedStars}";

        Invoke(nameof(TriggerGameOverTransition), 0.25f);
    }

    private void TriggerGameOverTransition()
    {
        ScreenTransition.Instance.PlayTransition(() =>
        {
            SetPanelImmediate(hudPanel, false);
            SetPanelImmediate(tutorialPanel, false);
            SetPanelImmediate(gameOverPanel, true);
        });
    }

    private void OnRestartClicked()
    {
        ScreenTransition.Instance.PlayTransition(() =>
        {
            GameManager.Instance?.RestartGame();
            SetPanelImmediate(gameOverPanel, false);
            SetPanelImmediate(mainMenuPanel, true);
            UpdateMenuData();
        });
    }

    private void UpdateScoreHUD(int newScore)
    {
        if (gameplayScoreText != null)
        {
            gameplayScoreText.text = newScore.ToString();

            if (newScore > 0 && scoreParticlePrefab != null)
            {
                Vector3 centerPosition = gameplayScoreText.rectTransform.TransformPoint(
                    gameplayScoreText.rectTransform.rect.center
                );
                Instantiate(scoreParticlePrefab, centerPosition, Quaternion.identity);
            }
        }
    }

    public void SetPanelImmediate(GameObject panel, bool active)
    {
        if (panel != null) panel.SetActive(active);
    }
}