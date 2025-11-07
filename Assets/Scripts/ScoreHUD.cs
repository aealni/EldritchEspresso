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
        
        // Create style - simpler approach
        GUIStyle mainStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.UpperCenter,
            fontSize = fontSize,
            fontStyle = FontStyle.Bold,
            normal = new GUIStyleState { textColor = textColor }
        };

        // Use a fixed width rect to avoid sizing issues
        float width = 200f; // Wide enough for "Score: 999999"
        float height = fontSize + 10f;
        float x = (Screen.width - width) / 2f;
        float y = margin.y;
        Rect rect = new Rect(x, y, width, height);

        // Shadow for readability
        Color originalColor = GUI.contentColor;
        GUI.contentColor = shadowColor;
        GUI.Label(new Rect(rect.x + 1, rect.y + 1, rect.width, rect.height), text, mainStyle);
        
        // Main text
        GUI.contentColor = textColor;
        GUI.Label(rect, text, mainStyle);
        GUI.contentColor = originalColor;
    }
}
