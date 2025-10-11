using UnityEngine;

public class TestGameState : MonoBehaviour
{
    void OnEnable()
    {
        GameStateManager.OnGameStateChange += HandleGameStateChange;
    }

    void OnDisable()
    {
        GameStateManager.OnGameStateChange -= HandleGameStateChange;
    }

    void HandleGameStateChange(GameStateManager.GameState newState)
    {
        // This method is automatically called when the state changes
        switch (newState)
        {
            case GameStateManager.GameState.Intro:
                Debug.Log("Intro State");
                break;
            case GameStateManager.GameState.Service:
                Debug.Log("Service State");
                break;
            case GameStateManager.GameState.Result:
                Debug.Log("Result State");
                break;
            case GameStateManager.GameState.Upgrade:
                Debug.Log("Upgrade State");
                break;
        }
    }
/*
    void Start()
    {
        // Example of changing states
        GameStateManager.Instance.ChangeState(GameStateManager.GameState.Intro);
        GameStateManager.Instance.ChangeState(GameStateManager.GameState.Service);
        GameStateManager.Instance.ChangeState(GameStateManager.GameState.Result);
        GameStateManager.Instance.ChangeState(GameStateManager.GameState.Upgrade);
    }
    */
}
