using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public DiscountType? DiscountType { get; set; }
    public decimal? DiscountValue { get; set; }
    public string? SKU { get; set; }
    public string? Barcode { get; set; }
    public int StockQuantity { get; set; }
    public int? LowStockThreshold { get; set; } = 5;
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; }
    public bool IsTrending { get; set; }
    public bool IsBestSeller { get; set; }
    public bool HasVariants { get; set; }
    public string? Tags { get; set; }
    public string? Specifications { get; set; }
    public decimal? Weight { get; set; }
    public string? Dimensions { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public int ViewCount { get; set; }
    public int SoldCount { get; set; }
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public DateTime? FlashSaleStart { get; set; }
    public DateTime? FlashSaleEnd { get; set; }
    public decimal? FlashSalePrice { get; set; }

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public int? BrandId { get; set; }
    public Brand? Brand { get; set; }

    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    public ICollection<ProductTag> ProductTags { get; set; } = new List<ProductTag>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<ProductQuestion> Questions { get; set; } = new List<ProductQuestion>();
}
