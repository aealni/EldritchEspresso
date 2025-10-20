using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Component for detecting nearby containers and managing container interactions
/// </summary>
public class ContainerDetector : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float detectionRange = 3f;
    [SerializeField] private LayerMask containerLayerMask = -1;
    [SerializeField] private float detectionInterval = 0.1f; // How often to check for containers
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;
    
    private List<Container> nearbyContainers = new List<Container>();
    private Container nearestContainer;
    private float lastDetectionTime;
    
    // Events
    public System.Action<Container> OnContainerEnterRange;
    public System.Action<Container> OnContainerExitRange;
    public System.Action<Container> OnNearestContainerChanged;
    
    // Properties
    public List<Container> NearbyContainers => new List<Container>(nearbyContainers);
    public Container NearestContainer => nearestContainer;
    public float DetectionRange => detectionRange;
    
    private void Update()
    {
        if (Time.time - lastDetectionTime >= detectionInterval)
        {
            UpdateNearbyContainers();
            lastDetectionTime = Time.time;
        }
    }
    
    /// <summary>
    /// Updates the list of nearby containers
    /// </summary>
    private void UpdateNearbyContainers()
    {
        // Find all containers in range
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, detectionRange, containerLayerMask);
        
        List<Container> currentContainers = new List<Container>();
        
        foreach (Collider2D collider in colliders)
        {
            Container container = collider.GetComponent<Container>();
            if (container != null)
            {
                currentContainers.Add(container);
                container.SetPlayerTransform(transform);
                
                if (!nearbyContainers.Contains(container))
                {
                    OnContainerEnterRange?.Invoke(container);
                    if (showDebugInfo)
                    {
                        Debug.Log($"Container entered range: {container.ContainerName}");
                    }
                }
            }
        }
        
        // Check for containers that are no longer in range
        foreach (Container container in nearbyContainers)
        {
            if (!currentContainers.Contains(container))
            {
                OnContainerExitRange?.Invoke(container);
                if (showDebugInfo)
                {
                    Debug.Log($"Container exited range: {container.ContainerName}");
                }
            }
        }
        
        nearbyContainers = currentContainers;
        
        // Update nearest container
        UpdateNearestContainer();
    }
    
    /// <summary>
    /// Updates the nearest container
    /// </summary>
    private void UpdateNearestContainer()
    {
        Container newNearest = null;
        float closestDistance = float.MaxValue;
        
        foreach (Container container in nearbyContainers)
        {
            float distance = container.GetInteractionDistance(transform);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                newNearest = container;
            }
        }
        
        if (newNearest != nearestContainer)
        {
            nearestContainer = newNearest;
            OnNearestContainerChanged?.Invoke(nearestContainer);
            
            if (showDebugInfo)
            {
                if (nearestContainer != null)
                {
                    Debug.Log($"Nearest container changed to: {nearestContainer.ContainerName}");
                }
                else
                {
                    Debug.Log("No containers nearby");
                }
            }
        }
    }
    
    /// <summary>
    /// Gets all containers within a specific range
    /// </summary>
    /// <param name="range">Detection range</param>
    /// <returns>List of containers within range</returns>
    public List<Container> GetContainersInRange(float range)
    {
        List<Container> containers = new List<Container>();
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, range, containerLayerMask);
        
        foreach (Collider2D collider in colliders)
        {
            Container container = collider.GetComponent<Container>();
            if (container != null)
            {
                containers.Add(container);
            }
        }
        
        return containers;
    }
    
    /// <summary>
    /// Gets the nearest container within a specific range
    /// </summary>
    /// <param name="range">Detection range</param>
    /// <returns>Nearest container or null if none found</returns>
    public Container GetNearestContainerInRange(float range)
    {
        List<Container> containers = GetContainersInRange(range);
        Container nearest = null;
        float closestDistance = float.MaxValue;
        
        foreach (Container container in containers)
        {
            float distance = container.GetInteractionDistance(transform);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                nearest = container;
            }
        }
        
        return nearest;
    }
    
    /// <summary>
    /// Checks if a specific container is within range
    /// </summary>
    /// <param name="container">Container to check</param>
    /// <returns>True if container is within range</returns>
    public bool IsContainerInRange(Container container)
    {
        if (container == null) return false;
        return nearbyContainers.Contains(container);
    }
    
    /// <summary>
    /// Gets the distance to a specific container
    /// </summary>
    /// <param name="container">Container to measure distance to</param>
    /// <returns>Distance to container or float.MaxValue if container is null</returns>
    public float GetDistanceToContainer(Container container)
    {
        if (container == null) return float.MaxValue;
        return container.GetInteractionDistance(transform);
    }
    
    /// <summary>
    /// Forces an immediate update of nearby containers
    /// </summary>
    public void ForceUpdate()
    {
        UpdateNearbyContainers();
    }
    
    /// <summary>
    /// Sets the detection range
    /// </summary>
    /// <param name="range">New detection range</param>
    public void SetDetectionRange(float range)
    {
        detectionRange = range;
    }
    
    /// <summary>
    /// Sets the layer mask for container detection
    /// </summary>
    /// <param name="layerMask">New layer mask</param>
    public void SetContainerLayerMask(LayerMask layerMask)
    {
        containerLayerMask = layerMask;
    }
    
    protected virtual void OnDrawGizmosSelected()
    {
        // Draw detection range
        // Gizmos.color = Color.cyan;
        // Gizmos.DrawWireCircle(transform.position, detectionRange);
        
        // Draw lines to nearby containers
        Gizmos.color = Color.yellow;
        foreach (Container container in nearbyContainers)
        {
            if (container != null)
            {
                Gizmos.DrawLine(transform.position, container.transform.position);
            }
        }
        
        // Draw line to nearest container
        if (nearestContainer != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, nearestContainer.transform.position);
        }
    }
    
    protected virtual void OnDrawGizmos()
    {
        // Draw detection range in a lighter color when not selected
        // Gizmos.color = new Color(0, 1, 1, 0.1f);
        // Gizmos.DrawCircle(transform.position, detectionRange);
    }
}
