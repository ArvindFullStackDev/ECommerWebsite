using Domain.Common;

namespace Domain.Entities;

public class CompareProduct : BaseEntity
{
    public string UserId { get; set; } = string.Empty;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
}
