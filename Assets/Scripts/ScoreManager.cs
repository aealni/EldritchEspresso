using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScoreManager : MonoBehaviour
{
    private static ScoreManager instance;
    private static bool applicationIsQuitting;

    public static ScoreManager Instance
    {
        get
        {
            if (applicationIsQuitting)
            {
                return null;
            }

            if (instance == null)
            {
                instance = FindFirstObjectByType<ScoreManager>();

                if (instance == null)
                {
                    var managerObject = new GameObject(nameof(ScoreManager));
                    instance = managerObject.AddComponent<ScoreManager>();
                }
            }

            return instance;
        }
        private set => instance = value;
    }

    private readonly Dictionary<string, int> miniGameScores = new Dictionary<string, int>();
    private int currentScore;
    private int dayHighScore;
    private int dayStartingScore;

    [Header("Day Result Display")]
    [SerializeField] private Vector2 dayResultBoxSize = new Vector2(220f, 80f);
    [SerializeField] private Vector2 dayResultBoxOffset = new Vector2(20f, 20f);
    [SerializeField] private float dayResultDisplayDuration = 3f;

    private bool showDayResultBox;
    private float dayResultTimer;
    private int lastDayScore;
    private GUIStyle dayResultBoxStyle;

    public event Action<int> OnScoreChanged;
    public event Action<int> OnDayHighScoreChanged;
    public event Action<int> OnDayFinished;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        StartNewDay();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureInstanceOnLoad()
    {
        // Force-create the singleton early so it is present in scenes
        // even if nothing directly references it yet.
        _ = Instance;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void OnApplicationQuit()
    {
        applicationIsQuitting = true;
    }

    private void OnEnable()
    {
        GameStateManager.OnGameStateChange += HandleGameStateChange;
    }

    private void OnDisable()
    {
        GameStateManager.OnGameStateChange -= HandleGameStateChange;
    }

    public IReadOnlyDictionary<string, int> MiniGameScores => miniGameScores;
    public int CurrentScore => currentScore;
    public int DayHighScore => dayHighScore;
    public int CurrentDayScore => currentScore - dayStartingScore;

    public void AddMiniGameScore(string miniGameId, int amount)
    {
        if (string.IsNullOrWhiteSpace(miniGameId))
        {
            Debug.LogWarning("ScoreManager.AddMiniGameScore called with an empty miniGameId.");
            return;
        }

        if (amount <= 0)
        {
            Debug.LogWarning($"ScoreManager.AddMiniGameScore ignored non-positive amount {amount} for mini-game '{miniGameId}'.");
            return;
        }

        if (miniGameScores.TryGetValue(miniGameId, out var existingScore))
        {
            miniGameScores[miniGameId] = existingScore + amount;
        }
        else
        {
            miniGameScores.Add(miniGameId, amount);
        }

        ApplyScoreDelta(amount);
    }

    public void EndDay()
    {
        var dayScore = CurrentDayScore;

        if (dayScore > dayHighScore)
        {
            dayHighScore = dayScore;
            OnDayHighScoreChanged?.Invoke(dayHighScore);
        }

        // Only show the result box in CafeScene, as requested
        if (SceneManager.GetActiveScene().name == "CafeScene")
        {
            ShowDayResult(dayScore);
        }
        OnDayFinished?.Invoke(dayScore);
        StartNewDay();
    }

    public void StartNewDay()
    {
        dayStartingScore = currentScore;

        OnScoreChanged?.Invoke(currentScore);
        OnDayHighScoreChanged?.Invoke(dayHighScore);
    }

    public int GetCurrentScore()
    {
        return currentScore;
    }

    public void ApplyScoreDelta(int amount)
    {
        currentScore += amount;
        OnScoreChanged?.Invoke(currentScore);

        var dayScore = CurrentDayScore;

        if (dayScore > dayHighScore)
        {
            dayHighScore = dayScore;
            OnDayHighScoreChanged?.Invoke(dayHighScore);
        }
    }

    private void Update()
    {
        if (!showDayResultBox)
        {
            return;
        }

        if (dayResultDisplayDuration <= 0f)
        {
            return;
        }

        dayResultTimer -= Time.unscaledDeltaTime;

        if (dayResultTimer <= 0f)
        {
            showDayResultBox = false;
        }
    }

    private void OnGUI()
    {
        if (!showDayResultBox)
        {
            return;
        }

        EnsureDayResultStyle();

        var boxRect = new Rect(dayResultBoxOffset.x, dayResultBoxOffset.y, dayResultBoxSize.x, dayResultBoxSize.y);
        GUI.Box(boxRect, $"Day Complete!\nScore: {lastDayScore}", dayResultBoxStyle);
    }

    private void ShowDayResult(int dayScore)
    {
        lastDayScore = dayScore;
        showDayResultBox = true;

        if (dayResultDisplayDuration > 0f)
        {
            dayResultTimer = dayResultDisplayDuration;
        }
    }

    private void EnsureDayResultStyle()
    {
        if (dayResultBoxStyle != null)
        {
            return;
        }

        dayResultBoxStyle = new GUIStyle(GUI.skin.box)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 18,
            wordWrap = true
        };
        dayResultBoxStyle.padding = new RectOffset(12, 12, 12, 12);
    }

    private void HandleGameStateChange(GameStateManager.GameState state)
    {
        // When the game enters the Result phase, end the day and show score
        if (state == GameStateManager.GameState.Result)
        {
            EndDay();
        }
    }
}
