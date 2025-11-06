using UnityEngine;
using System.Collections;

public class GameTime : MonoBehaviour
{
    public static int gameTimeSeconds;
    public static int dayCount = 1; 

    private bool isPaused = false;

    private Coroutine timerCoroutine;

    void Start()
    {
        ResetTime(); 
    }


    public void ResetTime()
    {
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
        }
        
        gameTimeSeconds = 0;
        isPaused = false;
        timerCoroutine = StartCoroutine(GameTimer());
        Debug.Log("GameTime: Time reset and timer started.");
    }

    public void PauseTime()
    {
        isPaused = true;
        Debug.Log("GameTime: Paused.");
    }

    public void ResumeTime()
    {
        isPaused = false;
        Debug.Log("GameTime: Resumed.");
    }

    IEnumerator GameTimer()
    {
        while (true)
        {
            yield return new WaitUntil(() => !isPaused);
            yield return new WaitForSeconds(1f);
            gameTimeSeconds += 1;
            //Debug.Log("Game Time: " + gameTimeSeconds + " seconds");
            gameTime += 1f;
            //Debug.Log("Game Time: " + gameTime + " seconds");
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
                    // Reset the day timer so the cycle repeats
                    gameTime = 0f;
                    break;
                default:
                    break;
            }
        }
    }
}
