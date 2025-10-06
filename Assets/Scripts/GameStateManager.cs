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
    public void Awake()
    {
        Debug.Log("GameStateManager Awake");
    }

    [SerializeField]
    public GameState currentState;
    private static GameStateManager instance;
    public static event Action<GameState> OnGameStateChange;

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