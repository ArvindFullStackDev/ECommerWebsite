using Domain.Common;

namespace Domain.Entities;

public class ReviewLike : BaseEntity
{
    public int ReviewId { get; set; }
    public Review Review { get; set; } = null!;

    public string UserId { get; set; } = string.Empty;
}
