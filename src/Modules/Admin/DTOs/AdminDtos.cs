namespace Admin.DTOs;

public class UserManagementDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public bool IsLocked { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<string> Roles { get; set; } = new();
}

public class CategoryManagementDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public int? ParentId { get; set; }
    public string? ParentName { get; set; }
    public int ProductCount { get; set; }
    public bool IsActive { get; set; }
}

public class ProductManagementDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? DiscountedPrice { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; }
}
