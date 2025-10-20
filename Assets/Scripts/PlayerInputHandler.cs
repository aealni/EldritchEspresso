using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles player input for inventory and container interactions
/// </summary>
public class PlayerInputHandler : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField] private bool useNewInputSystem = false; // Disabled until InputSystem is properly set up
    
    // New Input System (commented out until InputSystem_Actions is generated)
    // private InputSystem_Actions inputActions;
    
    // Events
    public System.Action OnSlot1Pressed;
    public System.Action OnSlot2Pressed;
    public System.Action OnSlot3Pressed;
    public System.Action OnSlot4Pressed;
    public System.Action OnEmptySlotPressed;
    public System.Action OnAddFromContainerPressed;
    public System.Action OnInteractPressed;
    
    private void Awake()
    {
        // InputSystem setup will be added when InputSystem_Actions is generated
        if (useNewInputSystem)
        {
            // inputActions = new InputSystem_Actions();
            Debug.LogWarning("New Input System is not yet configured. Using legacy input.");
        }
    }
    
    private void OnEnable()
    {
        // InputSystem enable will be added when InputSystem_Actions is generated
        if (useNewInputSystem)
        {
            // if (inputActions != null)
            // {
            //     inputActions.Enable();
            //     BindInputActions();
            // }
        }
    }
    
    private void OnDisable()
    {
        // InputSystem disable will be added when InputSystem_Actions is generated
        if (useNewInputSystem)
        {
            // if (inputActions != null)
            // {
            //     inputActions.Disable();
            //     UnbindInputActions();
            // }
        }
    }
    
    private void Update()
    {
        if (!useNewInputSystem)
        {
            HandleLegacyInput();
        }
    }
    
    /// <summary>
    /// Binds input actions for the new Input System
    /// </summary>
    private void BindInputActions()
    {
        // Will be implemented when InputSystem_Actions is generated
        // if (inputActions == null) return;
        
        // Note: These actions need to be added to the InputSystem_Actions.inputactions file
        // For now, we'll use legacy input as fallback
        Debug.Log("New Input System actions would be bound here");
    }
    
    /// <summary>
    /// Unbinds input actions
    /// </summary>
    private void UnbindInputActions()
    {
        // Will be implemented when InputSystem_Actions is generated
        // if (inputActions == null) return;
        
        Debug.Log("New Input System actions would be unbound here");
    }
    
    /// <summary>
    /// Handles input using the legacy Input Manager
    /// </summary>
    private void HandleLegacyInput()
    {
        // Inventory slot selection (keys 1-4)
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            OnSlot1Pressed?.Invoke();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            OnSlot2Pressed?.Invoke();
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            OnSlot3Pressed?.Invoke();
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            OnSlot4Pressed?.Invoke();
        }
        
        // Empty selected slot (key E)
        if (Input.GetKeyDown(KeyCode.E))
        {
            OnEmptySlotPressed?.Invoke();
        }
        
        // Add from container (key Q)
        if (Input.GetKeyDown(KeyCode.Q))
        {
            OnAddFromContainerPressed?.Invoke();
        }
        
        // Interact with container (key F or Interact action)
        if (Input.GetKeyDown(KeyCode.F))
        {
            OnInteractPressed?.Invoke();
        }
    }
    
    /// <summary>
    /// Sets whether to use the new Input System
    /// </summary>
    /// <param name="useNew">True to use new Input System, false for legacy</param>
    public void SetUseNewInputSystem(bool useNew)
    {
        useNewInputSystem = useNew;
        
        if (useNewInputSystem)
        {
            // Will be implemented when InputSystem_Actions is generated
            // if (inputActions == null)
            // {
            //     inputActions = new InputSystem_Actions();
            // }
            // inputActions.Enable();
            // BindInputActions();
            Debug.LogWarning("New Input System is not yet configured. Using legacy input.");
        }
        else
        {
            // Will be implemented when InputSystem_Actions is generated
            // if (inputActions != null)
            // {
            //     inputActions.Disable();
            //     UnbindInputActions();
            // }
        }
    }
    
    /// <summary>
    /// Gets the current input system being used
    /// </summary>
    /// <returns>True if using new Input System, false if using legacy</returns>
    public bool IsUsingNewInputSystem()
    {
        return useNewInputSystem;
    }
    
    /// <summary>
    /// Manually trigger slot selection (for UI buttons, etc.)
    /// </summary>
    /// <param name="slotIndex">Slot index (0-3)</param>
    public void TriggerSlotSelection(int slotIndex)
    {
        switch (slotIndex)
        {
            case 0:
                OnSlot1Pressed?.Invoke();
                break;
            case 1:
                OnSlot2Pressed?.Invoke();
                break;
            case 2:
                OnSlot3Pressed?.Invoke();
                break;
            case 3:
                OnSlot4Pressed?.Invoke();
                break;
            default:
                Debug.LogWarning($"Invalid slot index: {slotIndex}");
                break;
        }
    }
    
    /// <summary>
    /// Manually trigger empty slot action
    /// </summary>
    public void TriggerEmptySlot()
    {
        OnEmptySlotPressed?.Invoke();
    }
    
    /// <summary>
    /// Manually trigger add from container action
    /// </summary>
    public void TriggerAddFromContainer()
    {
        OnAddFromContainerPressed?.Invoke();
    }
    
    /// <summary>
    /// Manually trigger interact action
    /// </summary>
    public void TriggerInteract()
    {
        OnInteractPressed?.Invoke();
    }
    
    private void OnDestroy()
    {
        // Will be implemented when InputSystem_Actions is generated
        // if (inputActions != null)
        // {
        //     inputActions.Dispose();
        // }
    }
}
