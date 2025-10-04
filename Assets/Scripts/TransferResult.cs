/// <summary>
/// Represents the result of a transfer operation between storage systems
/// </summary>
public enum TransferResult
{
    /// <summary>
    /// Transfer completed successfully with all items moved
    /// </summary>
    Success,
    
    /// <summary>
    /// Transfer partially completed - some items were moved but not all
    /// </summary>
    PartialSuccess,
    
    /// <summary>
    /// Transfer failed because the destination inventory is full
    /// </summary>
    InventoryFull,
    
    /// <summary>
    /// Transfer failed because the requested item was not found in source
    /// </summary>
    ItemNotFound,
    
    /// <summary>
    /// Transfer failed due to insufficient quantity in source
    /// </summary>
    InsufficientQuantity,
    
    /// <summary>
    /// Transfer failed due to invalid parameters (null references, negative quantities, etc.)
    /// </summary>
    InvalidParameters
}
