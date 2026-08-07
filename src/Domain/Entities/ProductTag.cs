using Domain.Common;

namespace Domain.Entities;

public class ProductTag : BaseEntity
{
    public string Tag { get; set; } = string.Empty;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
}
