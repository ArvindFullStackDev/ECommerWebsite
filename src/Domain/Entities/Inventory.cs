using Domain.Common;

namespace Domain.Entities;

public class Inventory : BaseEntity
{
    public int Quantity { get; set; }
    public int ReservedQuantity { get; set; }
    public int AvailableQuantity => Quantity - ReservedQuantity;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public ICollection<StockHistory> StockHistories { get; set; } = new List<StockHistory>();
}
