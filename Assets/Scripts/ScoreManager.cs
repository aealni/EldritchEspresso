using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
  public static ScoreManager Instance { get; private set; }

  public event Action<int> OnScoreChanged;
  public event Action<int> OnHighScoreChanged;
}
