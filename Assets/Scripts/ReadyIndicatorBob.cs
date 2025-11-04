using UnityEngine;

/// <summary>
/// Simple bobbing animation for a ready indicator (e.g., an exclamation mark) without needing an Animator.
/// </summary>
[DisallowMultipleComponent]
public class ReadyIndicatorBob : MonoBehaviour
{
    [SerializeField] private float amplitude = 0.05f;
    [SerializeField] private float frequency = 3f;

    private Vector3 baseLocalPos;

    private void OnEnable()
    {
        baseLocalPos = transform.localPosition;
    }

    private void OnDisable()
    {
        transform.localPosition = baseLocalPos;
    }

    private void Update()
    {
        transform.localPosition = baseLocalPos + Vector3.up * (Mathf.Sin(Time.time * frequency) * amplitude);
    }
}
