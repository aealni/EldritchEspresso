using UnityEngine;

/// <summary>
/// Simple green "border" style highlight for 2D sprites.
/// It clones the target SpriteRenderer as a slightly larger, green-tinted child and toggles it on proximity.
/// </summary>
[DisallowMultipleComponent]
public class ProximityHighlighter : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private SpriteRenderer target;

    [Header("Style")] 
    [SerializeField] private Color borderColor = new Color(0.2f, 1f, 0.2f, 0.85f);
    [SerializeField] private float scaleMultiplier = 1.08f; // slightly larger than the target
    [SerializeField] private int sortingOrderOffset = -1; // draw behind by default

    private GameObject outlineGO;
    private SpriteRenderer outlineSR;

    private void Awake()
    {
        if (target == null)
            target = GetComponent<SpriteRenderer>();

        EnsureOutline();
        Hide();
    }

    private void EnsureOutline()
    {
        if (outlineGO != null && outlineSR != null) return;

        outlineGO = new GameObject("HighlightOutline");
        outlineGO.transform.SetParent(transform, false);
        outlineGO.transform.localPosition = Vector3.zero;
        outlineGO.transform.localRotation = Quaternion.identity;
        outlineGO.transform.localScale = Vector3.one * scaleMultiplier;

        outlineSR = outlineGO.AddComponent<SpriteRenderer>();
        if (target != null)
        {
            outlineSR.sprite = target.sprite;
            outlineSR.sortingLayerID = target.sortingLayerID;
            outlineSR.sortingOrder = target.sortingOrder + sortingOrderOffset;
        }
        outlineSR.color = borderColor;
        outlineSR.enabled = false;
    }

    private void LateUpdate()
    {
        // Keep sprite/ordering in sync in case it changes
        if (target != null && outlineSR != null)
        {
            outlineSR.sprite = target.sprite;
            outlineSR.sortingLayerID = target.sortingLayerID;
            outlineSR.sortingOrder = target.sortingOrder + sortingOrderOffset;
        }
    }

    public void Show()
    {
        if (outlineSR == null) EnsureOutline();
        outlineGO.transform.localScale = Vector3.one * scaleMultiplier;
        outlineSR.enabled = true;
    }

    public void Hide()
    {
        if (outlineSR != null)
            outlineSR.enabled = false;
    }
}
