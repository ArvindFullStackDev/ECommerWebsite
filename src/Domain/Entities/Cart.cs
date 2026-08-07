using Domain.Common;

namespace Domain.Entities;

public class Cart : BaseEntity
{
    public string UserId { get; set; } = string.Empty;

    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
}
