using UnityEngine;

/// <summary>
/// Lightweight HUD that shows the current score at the top-right corner using OnGUI.
/// Avoids requiring a Canvas/Text asset and works immediately in any scene.
/// </summary>
[DisallowMultipleComponent]
public class ScoreHUD : MonoBehaviour
{
    [SerializeField] private Vector2 margin = new Vector2(16f, 10f);
    [SerializeField] private int fontSize = 20;
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private Color shadowColor = new Color(0f, 0f, 0f, 0.6f);

    private void OnGUI()
    {
        // Get score first
        int score = ScoreManager.Instance ? ScoreManager.Instance.GetCurrentScore() : 0;
        string text = $"Score: {score}";
        
        // Recreate style each frame to ensure it works in builds
        GUIStyle mainStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.UpperCenter,
            fontSize = fontSize,
            fontStyle = FontStyle.Bold
        };
        mainStyle.normal.textColor = textColor;

        Vector2 size = mainStyle.CalcSize(new GUIContent(text));

        float x = (Screen.width - size.x) / 2f;
        float y = margin.y;
        Rect rect = new Rect(x, y, size.x, size.y);

        // Shadow for readability
        GUIStyle shadowStyle = new GUIStyle(mainStyle);
        shadowStyle.normal.textColor = shadowColor;
        GUI.Label(new Rect(rect.x + 1, rect.y + 1, rect.width, rect.height), text, shadowStyle);

        // Main text
        GUI.Label(rect, text, mainStyle);
    }
}
