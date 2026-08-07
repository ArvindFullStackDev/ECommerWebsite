using Domain.Common;

namespace Domain.Entities;

public class StockHistory : BaseEntity
{
    public int QuantityChange { get; set; }
    public int PreviousStock { get; set; }
    public int NewStock { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }

    public int InventoryId { get; set; }
    public Inventory Inventory { get; set; } = null!;
}
