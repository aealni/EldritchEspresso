using UnityEngine;
using System.Collections;

public class GameTime : MonoBehaviour
{
    
    [SerializeField] private float gameTime;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Debug.Log("Game Time Started");
        gameTime = 0f;
        StartCoroutine(gameTimer());
    }

    // Update is called once per frame
    void Update()
    {


    }
    IEnumerator gameTimer() {
        while (true)
        {
            yield return new WaitForSeconds(1f);
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
                    break;
                default:
                    break;
            }
        }
    }
}
