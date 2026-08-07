using Domain.Common;

namespace Domain.Entities;

public class Review : BaseEntity
{
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public string? ImageUrl { get; set; }
    public string? VideoUrl { get; set; }
    public bool IsApproved { get; set; }
    public int LikeCount { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public string UserId { get; set; } = string.Empty;

    public ICollection<ReviewLike> ReviewLikes { get; set; } = new List<ReviewLike>();
}
