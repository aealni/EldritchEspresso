using UnityEngine;
using System;

public class GameStateManager : MonoBehaviour
{
    public enum GameState
    {
        Intro,
        Service,
        Result,
        Upgrade
    }   

    private GameTime gameTimeInstance;
    [SerializeField]
    public GameState currentState;
    private static GameStateManager instance;
    public static event Action<GameState> OnGameStateChange;

    public void Awake()
    {
        Debug.Log("GameStateManager Awake");
        OnGameStateChange += HandleGameStateChange;
        gameTimeInstance = FindFirstObjectByType<GameTime>(); 
        if (gameTimeInstance == null)
        {
            Debug.LogError("GameStateManager: Could not find GameTime component!");
        }
    }
    public void OnDestroy()
    {
        OnGameStateChange -= HandleGameStateChange;
    }
    void HandleGameStateChange(GameState newState)
    {
        // functionality moved from TestGameState
        switch (newState)
        {
            case GameStateManager.GameState.Intro:
                GameTime.dayCount++;
                Debug.Log("GameStateManager: State changed to Intro.");
                gameTimeInstance.ResetTime();
                break;
            case GameStateManager.GameState.Service:
                Debug.Log("GameStateManager: State changed to Service.");
                gameTimeInstance.ResumeTime();
                break;
            case GameStateManager.GameState.Result:
                Debug.Log("GameStateManager: State changed to Result.");
                gameTimeInstance.PauseTime();
                break;
            case GameStateManager.GameState.Upgrade:
                Debug.Log("GameStateManager: State changed to Upgrade.");
                break;
        }
    }
    
    public void ChangeState(GameState state)
    {
        currentState = state;
        OnGameStateChange?.Invoke(state);
    }

    public GameState GetCurrentState()
    {
        Debug.Log("GetCurrentState called");
        return currentState;
    }

    public static GameStateManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<GameStateManager>();
                if (instance == null)
                {
                    GameObject obj = new GameObject("GameStateManager");
                    instance = obj.AddComponent<GameStateManager>();
                }
            }
            return instance;
        }
    }
    
    
}