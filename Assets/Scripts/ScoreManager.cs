using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
  public static ScoreManager Instance { get; private set; }

  private int currentScore = 0;

  public event Action<int> OnScoreChanged;
  public event Action<int> OnHighScoreChanged;

  void Awake() 
  {
    if (Instance != null && Instance != this) 
    {
      Destroy(gameObject);
      return;
    }
    Instance = this;
    DontDestroyOnLoad(gameObject);
  }
  

  public void addScore(int amount) 
    {
      currentScore += amount;
    }

  public void ResetScore()
    {
      currentScore = 0;
    }
  public int getCurrentScore()
    {
      return currentScore;
    }
}
