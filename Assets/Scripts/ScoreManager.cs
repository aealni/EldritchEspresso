using System;
using System.Collections.Generic;
using UnityEngine;

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
                instance = FindObjectOfType<ScoreManager>();

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

    public IReadOnlyDictionary<string, int> MiniGameScores => miniGameScores;
    public int CurrentScore => currentScore;
    public int DayHighScore => dayHighScore;

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
        OnDayFinished?.Invoke(dayHighScore);
        StartNewDay();
    }

    public void StartNewDay()
    {
        currentScore = 0;
        dayHighScore = 0;
        miniGameScores.Clear();

        OnScoreChanged?.Invoke(currentScore);
        OnDayHighScoreChanged?.Invoke(dayHighScore);
    }

    public void ResetScore()
    {
        StartNewDay();
    }

    public int GetCurrentScore()
    {
        return currentScore;
    }

    private void ApplyScoreDelta(int amount)
    {
        currentScore += amount;
        OnScoreChanged?.Invoke(currentScore);

        if (currentScore > dayHighScore)
        {
            dayHighScore = currentScore;
            OnDayHighScoreChanged?.Invoke(dayHighScore);
        }
    }
}
