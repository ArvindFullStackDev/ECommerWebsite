namespace Inventory.DTOs;

public class StockDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int QuantityInStock { get; set; }
    public int ReservedQuantity { get; set; }
    public int AvailableQuantity => QuantityInStock - ReservedQuantity;
    public int LowStockThreshold { get; set; } = 5;
    public bool IsLowStock => AvailableQuantity <= LowStockThreshold;
    public DateTime? LastRestockedAt { get; set; }
}

public class StockHistoryDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ChangeType { get; set; } = string.Empty;
    public int QuantityChanged { get; set; }
    public int NewQuantity { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
