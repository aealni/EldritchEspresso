using UnityEngine;

public class GameTime : MonoBehaviour
{
    private float GameTime
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameTime = 0f;
        StartCoroutine(GameTimer());
    }

    // Update is called once per frame
    void Update()
    {


    }
    IEnumerator GameTimer() {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            GameTime += 1f;
            Debug.Log("Game Time: " + GameTime + " seconds");
            switch (gameTime)
            { 
                case 60f:
                    GameStateManager.Instance.ChangeState(GameStateManager.GameState.Service);
                    break;
                case 120f:
                    GameStateManager.Instance.ChangeState(GameStateManager.GameState.Result);
                    break;
                case 150f:
                    GameStateManager.Instance.ChangeState(GameStateManager.GameState.Upgrade);
                    break;
                default:
                    break;
            }
        }
    }
}
