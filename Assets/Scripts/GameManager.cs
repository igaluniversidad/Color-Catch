using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState
    {
        Menu,
        WaitingToStart,
        Playing,
        Paused,
        GameOver
    }

    [Header("Estado de Juego")]
    public GameState CurrentState { get; private set; } = GameState.Menu;

    [Header("Puntuación")]
    public int CurrentScore { get; private set; } = 0;
    public int HighScore { get; private set; } = 0;

    [Header("Economía (Estrellas)")]
    public int TotalStars { get; private set; } = 0;
    public int StarsEarnedThisMatch { get; private set; } = 0;

    [Header("Rendimiento Móvil")]
    [SerializeField] private int targetFPS = 60;

    public static event Action OnEnterWaitingToStart;
    public static event Action OnGameStarted;
    public static event Action OnGamePaused;
    public static event Action OnGameResumed;
    public static event Action<int> OnScoreUpdated;
    public static event Action<int, int, int> OnGameOver; // Score, HighScore, StarsEarned
    public static event Action<int> OnStarsUpdated;

    private const string HighScoreKey = "ColorRing_HighScore";
    private const string StarsKey = "ColorRing_TotalStars";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = targetFPS;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        LoadData();
    }

    void Start()
    {
        Time.timeScale = 1f;
        CurrentState = GameState.Menu;
        CurrentScore = 0;
    }

    public void PrepareGame()
    {
        Time.timeScale = 1f;
        CurrentState = GameState.WaitingToStart;
        CurrentScore = 0;
        StarsEarnedThisMatch = 0;
        OnEnterWaitingToStart?.Invoke();
        OnScoreUpdated?.Invoke(CurrentScore);
    }

    public void StartGame()
    {
        if (CurrentState != GameState.WaitingToStart) return;

        Time.timeScale = 1f;
        CurrentState = GameState.Playing;
        OnGameStarted?.Invoke();

        OrbSpawner spawner = FindFirstObjectByType<OrbSpawner>();
        if (spawner != null)
        {
            spawner.SetSpawning(true);
        }
    }

    public void PauseGame()
    {
        if (CurrentState != GameState.Playing) return;

        CurrentState = GameState.Paused;
        Time.timeScale = 0f;
        OnGamePaused?.Invoke();
    }

    public void ResumeGame()
    {
        if (CurrentState != GameState.Paused) return;

        Time.timeScale = 1f;
        CurrentState = GameState.Playing;
        OnGameResumed?.Invoke();
    }

    public void AddScore(int amount = 1)
    {
        if (CurrentState != GameState.Playing) return;

        CurrentScore += amount;

        // 1 Estrella por cada 10 puntos (al llegar a 10, 20, 30...)
        if (CurrentScore > 0 && CurrentScore % 10 == 0)
        {
            AddStars(1);
            StarsEarnedThisMatch++;
        }

        if (CurrentScore > HighScore)
        {
            HighScore = CurrentScore;
            SaveData();
        }

        OnScoreUpdated?.Invoke(CurrentScore);
    }

    public void AddStars(int amount)
    {
        TotalStars += amount;
        SaveData();
        OnStarsUpdated?.Invoke(TotalStars);
    }

    public bool SpendStars(int amount)
    {
        if (TotalStars >= amount)
        {
            TotalStars -= amount;
            SaveData();
            OnStarsUpdated?.Invoke(TotalStars);
            return true;
        }
        return false;
    }

    public void GameOver()
    {
        if (CurrentState == GameState.GameOver) return;

        Time.timeScale = 1f;
        CurrentState = GameState.GameOver;

        OrbSpawner spawner = FindFirstObjectByType<OrbSpawner>();
        if (spawner != null)
        {
            spawner.SetSpawning(false);
            spawner.ClearAllActiveOrbs();
        }

        OnGameOver?.Invoke(CurrentScore, HighScore, StarsEarnedThisMatch);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        CurrentState = GameState.Menu;
        CurrentScore = 0;
        StarsEarnedThisMatch = 0;

        OrbSpawner spawner = FindFirstObjectByType<OrbSpawner>();
        if (spawner != null)
        {
            spawner.SetSpawning(false);
            spawner.ClearAllActiveOrbs();
        }
    }

    private void LoadData()
    {
        HighScore = PlayerPrefs.GetInt(HighScoreKey, 0);
        TotalStars = PlayerPrefs.GetInt(StarsKey, 0);
    }

    private void SaveData()
    {
        PlayerPrefs.SetInt(HighScoreKey, HighScore);
        PlayerPrefs.SetInt(StarsKey, TotalStars);
        PlayerPrefs.Save();
    }
#if UNITY_EDITOR
    // 1. Clic derecho sobre el componente GameManager en el Inspector -> "Add 50 Stars"
    [ContextMenu("Debug/Add 50 Stars")]
    public void DebugAddStars()
    {
        AddStars(50);
        Debug.Log($"[DEBUG] Se agregaron 50 estrellas. Total actual: {TotalStars}");
    }

    // 2. Clic derecho -> "Reset All Stars to 0"
    [ContextMenu("Debug/Reset Stars")]
    public void DebugResetStars()
    {
        TotalStars = 0;
        SaveData();
        OnStarsUpdated?.Invoke(TotalStars);
        Debug.Log("[DEBUG] Estrellas reiniciadas a 0.");
    }

    // 3. Atajo de teclado mientras pruebas en Play Mode (Presiona la tecla M para ganar 50 estrellas)
    void LateUpdate()
    {
        if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.mKey.wasPressedThisFrame)
        {
            DebugAddStars();
        }
    }
#endif
}