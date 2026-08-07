namespace Catalog.DTOs;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public string? DiscountType { get; set; }
    public decimal? DiscountValue { get; set; }
    public decimal? DisplayPrice => DiscountPrice ?? Price;
    public decimal? Savings => Price - (DiscountPrice ?? Price);
    public int SavingsPercent => Savings > 0 ? (int)((Savings / Price) * 100) : 0;
    public string? SKU { get; set; }
    public string? Barcode { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsTrending { get; set; }
    public bool IsBestSeller { get; set; }
    public string? Tags { get; set; }
    public string? Specifications { get; set; }
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public int SoldCount { get; set; }
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public int? BrandId { get; set; }
    public string? BrandName { get; set; }
    public string? PrimaryImageUrl { get; set; }
    public List<string> ImageUrls { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public bool HasFlashSale { get; set; }
    public decimal? FlashSalePrice { get; set; }
    public DateTime? FlashSaleStart { get; set; }
    public DateTime? FlashSaleEnd { get; set; }
}

public class CreateProductDto
{
    public string Name { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public string? DiscountType { get; set; }
    public decimal? DiscountValue { get; set; }
    public string? SKU { get; set; }
    public string? Barcode { get; set; }
    public int StockQuantity { get; set; }
    public int? LowStockThreshold { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; }
    public bool IsTrending { get; set; }
    public bool IsBestSeller { get; set; }
    public string? Tags { get; set; }
    public string? Specifications { get; set; }
    public int CategoryId { get; set; }
    public int? BrandId { get; set; }
}

public class UpdateProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public string? DiscountType { get; set; }
    public decimal? DiscountValue { get; set; }
    public string? SKU { get; set; }
    public string? Barcode { get; set; }
    public int StockQuantity { get; set; }
    public int? LowStockThreshold { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsTrending { get; set; }
    public bool IsBestSeller { get; set; }
    public string? Tags { get; set; }
    public string? Specifications { get; set; }
    public int CategoryId { get; set; }
    public int? BrandId { get; set; }
}

public class ProductSearchDto
{
    public string? SearchTerm { get; set; }
    public int? CategoryId { get; set; }
    public int? BrandId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int? MinRating { get; set; }
    public string? SortBy { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
    public bool? InStock { get; set; }
    public bool? IsFeatured { get; set; }
    public bool? IsTrending { get; set; }
    public bool? IsBestSeller { get; set; }
}
