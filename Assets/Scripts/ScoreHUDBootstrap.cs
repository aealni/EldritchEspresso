using UnityEngine;

/// <summary>
/// Ensures a ScoreHUD exists in the scene at runtime so score is visible without manual wiring.
/// </summary>
public static class ScoreHUDBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureScoreHUD()
    {
        if (Object.FindFirstObjectByType<ScoreHUD>() != null) return;
        var go = new GameObject("ScoreHUD");
        go.AddComponent<ScoreHUD>();
        Object.DontDestroyOnLoad(go);
    }
}
