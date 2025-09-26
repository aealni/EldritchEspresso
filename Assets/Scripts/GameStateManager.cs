using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public enum GameState
    {
        Intro,
        Service,
        Result,
        Upgrade
    }
    [SerializeField]
    public GameState currentState;
    private static GameStateManager instance;
    public void ChangeState(GameState state)
    {
        currentState = state;
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

