using Domain.Common;

namespace Domain.Entities;

public class ProductVariant : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public decimal? PriceAdjustment { get; set; }
    public int StockQuantity { get; set; }
    public string? SKU { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
}
