using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Paneles")]
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
    [SerializeField] private Button pauseShopButton; // Nuevo botón de tienda en pausa
    [SerializeField] private Button pauseMenuButton;

    [Header("Panel Game Over")]
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI bestScoreText;
    [SerializeField] private TextMeshProUGUI earnedStarsText;
    [SerializeField] private Button restartButton;

    // Recuerda de qué menú se abrió la tienda
    private bool openedShopFromPause = false;

    void Awake()
    {
        if (playButton != null) playButton.onClick.AddListener(OnPlayClicked);
        if (restartButton != null) restartButton.onClick.AddListener(OnRestartClicked);
        if (openShopButton != null) openShopButton.onClick.AddListener(OnOpenShopFromMenuClicked);
        if (pauseShopButton != null) pauseShopButton.onClick.AddListener(OnOpenShopFromPauseClicked);
        if (closeShopButton != null) closeShopButton.onClick.AddListener(OnCloseShopClicked);

        if (pauseButton != null) pauseButton.onClick.AddListener(OnPauseClicked);
        if (resumeButton != null) resumeButton.onClick.AddListener(OnResumeClicked);
        if (pauseMenuButton != null) pauseMenuButton.onClick.AddListener(OnPauseMenuClicked);
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
    }

    private void UpdateMenuData()
    {
        if (GameManager.Instance != null)
        {
            if (menuHighScoreText != null) menuHighScoreText.text = $"BEST: {GameManager.Instance.HighScore}";
            UpdateStarsUI(GameManager.Instance.TotalStars);
        }
    }

    private void UpdateStarsUI(int totalStars)
    {
        if (menuStarsText != null) menuStarsText.text = $" {totalStars}";
        if (shopStarsText != null) shopStarsText.text = $" {totalStars}";
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
        // 1. Ocultar pausa y terminar/limpiar la partida activa
        SetPanelImmediate(pausePanel, false);
        SetPanelImmediate(hudPanel, false);
        GameManager.Instance?.RestartGame();
        Time.timeScale = 1f;

        // 2. Transición hacia la tienda
        ScreenTransition.Instance.PlayTransition(() =>
        {
            SetPanelImmediate(shopPanel, true);
        });
    }

    private void OnCloseShopClicked()
    {
        // Al salir de la tienda siempre vuelve al Menú Principal
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
        if (earnedStarsText != null) earnedStarsText.text = $"+{earnedStars} ";

        Invoke(nameof(TriggerGameOverTransition), 0.2f);
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

    private void SetPanelImmediate(GameObject panel, bool active)
    {
        if (panel != null) panel.SetActive(active);
    }
}