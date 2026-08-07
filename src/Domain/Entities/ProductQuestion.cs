using Domain.Common;

namespace Domain.Entities;

public class ProductQuestion : BaseEntity
{
    public string Question { get; set; } = string.Empty;
    public string? Answer { get; set; }
    public bool IsAnswered { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public string UserId { get; set; } = string.Empty;
}
