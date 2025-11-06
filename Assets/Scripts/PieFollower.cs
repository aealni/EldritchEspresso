using UnityEngine;

/// <summary>
/// Makes a pie sprite follow the player on the ground when the player has a pie in inventory
/// </summary>
public class PieFollower : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Food pieItemToTrack; // The specific pie item to track
    
    [Header("Follower Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0, -0.5f, 0); // Offset behind/below player
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        // Start hidden
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
    }
    
    private void Start()
    {
        // Auto-find player if not set
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
                playerController = player.GetComponent<PlayerController>();
            }
        }
        
        // Try to find the pie item if not set
        if (pieItemToTrack == null)
        {
            // Look for any Oven in the scene and get its pie
            Oven oven = FindFirstObjectByType<Oven>();
            if (oven != null && oven.PieItem != null)
            {
                pieItemToTrack = oven.PieItem;
            }
        }
    }
    
    private void LateUpdate()
    {
        if (playerTransform == null || playerController == null || pieItemToTrack == null)
        {
            return;
        }
        
        // Check if player has any pie in inventory
        bool hasPie = playerController.Inventory.HasItem(pieItemToTrack);
        
        // Show/hide the sprite
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = hasPie;
        }
        
        // Follow player if pie is held
        if (hasPie)
        {
            Vector3 targetPosition = playerTransform.position + offset;
            transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
        }
    }
}
