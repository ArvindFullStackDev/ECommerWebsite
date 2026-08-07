using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;
using Shared.Constants;

namespace Persistence.Seeds;

public class ApplicationDbContextSeed
{
    public static async Task SeedAsync(ApplicationDbContext context, UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        await SeedRolesAsync(roleManager);
        context.ChangeTracker.Clear();
        await SeedUsersAsync(userManager);
        context.ChangeTracker.Clear();
        await SeedCategoriesAsync(context);
        context.ChangeTracker.Clear();
        await SeedBrandsAsync(context);
        context.ChangeTracker.Clear();
        await SeedProductsAsync(context);
        context.ChangeTracker.Clear();
        await SeedBannersAsync(context);
        context.ChangeTracker.Clear();
        await SeedSiteSettingsAsync(context);
        context.ChangeTracker.Clear();
        await SeedCouponsAsync(context);
        context.ChangeTracker.Clear();
        await SeedCmsPagesAsync(context);
        context.ChangeTracker.Clear();
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roles = { "SuperAdmin", "Admin", "Customer", "Seller" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    private static async Task SeedUsersAsync(UserManager<IdentityUser> userManager)
    {
        var adminEmail = "admin@ecommerce.com";
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var admin = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(admin, "Admin@123");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, "SuperAdmin");
        }

        var customerEmail = "customer@ecommerce.com";
        if (await userManager.FindByEmailAsync(customerEmail) == null)
        {
            var customer = new IdentityUser
            {
                UserName = customerEmail,
                Email = customerEmail,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(customer, "Customer@123");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(customer, "Customer");
        }
    }

    private static async Task SeedCategoriesAsync(ApplicationDbContext context)
    {
        if (await context.Categories.AnyAsync()) return;

        var electronics = new Category { Name = "Electronics", Slug = "electronics", Description = "Electronic devices and accessories", IsActive = true, DisplayOrder = 1 };
        var clothing = new Category { Name = "Clothing", Slug = "clothing", Description = "Fashion and apparel", IsActive = true, DisplayOrder = 2 };
        var homeGarden = new Category { Name = "Home & Garden", Slug = "home-garden", Description = "Home improvement and garden", IsActive = true, DisplayOrder = 3 };
        var books = new Category { Name = "Books", Slug = "books", Description = "Books and media", IsActive = true, DisplayOrder = 4 };
        var sports = new Category { Name = "Sports & Outdoors", Slug = "sports-outdoors", Description = "Sports equipment and outdoor gear", IsActive = true, DisplayOrder = 5 };

        context.Categories.AddRange(electronics, clothing, homeGarden, books, sports);

        var mobilePhones = new Category { Name = "Mobile Phones", Slug = "mobile-phones", ParentCategory = electronics, IsActive = true, DisplayOrder = 1 };
        var laptops = new Category { Name = "Laptops", Slug = "laptops", ParentCategory = electronics, IsActive = true, DisplayOrder = 2 };
        var headphones = new Category { Name = "Headphones", Slug = "headphones", ParentCategory = electronics, IsActive = true, DisplayOrder = 3 };
        var menClothing = new Category { Name = "Men's Clothing", Slug = "mens-clothing", ParentCategory = clothing, IsActive = true, DisplayOrder = 1 };
        var womenClothing = new Category { Name = "Women's Clothing", Slug = "womens-clothing", ParentCategory = clothing, IsActive = true, DisplayOrder = 2 };

        context.Categories.AddRange(mobilePhones, laptops, headphones, menClothing, womenClothing);
        await context.SaveChangesAsync();
    }

    private static async Task SeedBrandsAsync(ApplicationDbContext context)
    {
        if (await context.Brands.AnyAsync()) return;

        var brands = new List<Brand>
        {
            new() { Name = "Apple", Slug = "apple", Description = "Apple Inc.", IsActive = true },
            new() { Name = "Samsung", Slug = "samsung", Description = "Samsung Electronics", IsActive = true },
            new() { Name = "Sony", Slug = "sony", Description = "Sony Corporation", IsActive = true },
            new() { Name = "Nike", Slug = "nike", Description = "Nike Inc.", IsActive = true },
            new() { Name = "Adidas", Slug = "adidas", Description = "Adidas AG", IsActive = true },
            new() { Name = "Microsoft", Slug = "microsoft", Description = "Microsoft Corporation", IsActive = true },
            new() { Name = "Dell", Slug = "dell", Description = "Dell Technologies", IsActive = true },
            new() { Name = "HP", Slug = "hp", Description = "HP Inc.", IsActive = true }
        };

        context.Brands.AddRange(brands);
        await context.SaveChangesAsync();
    }

    private static async Task SeedProductsAsync(ApplicationDbContext context)
    {
        if (await context.Products.AnyAsync()) return;

        var category = await context.Categories.FirstOrDefaultAsync(c => c.Slug == "mobile-phones");
        var brand = await context.Brands.FirstOrDefaultAsync(b => b.Slug == "apple");

        if (category != null && brand != null)
        {
            var product = new Product
            {
                Name = "iPhone 15 Pro Max",
                Slug = "iphone-15-pro-max",
                ShortDescription = "The most powerful iPhone ever with A17 Pro chip",
                Description = "Experience the power of A17 Pro chip with 6-core GPU. Pro camera system with 48MP main camera, Ultra Wide, and Telephoto. Super Retina XDR display with ProMotion. All-day battery life.",
                Price = 1199.99m,
                DiscountPrice = 1099.99m,
                DiscountType = DiscountType.FixedAmount,
                DiscountValue = 100,
                SKU = "APP-IP15PM-256",
                Barcode = "1234567890123",
                StockQuantity = 100,
                LowStockThreshold = 10,
                IsActive = true,
                IsFeatured = true,
                IsTrending = true,
                IsBestSeller = true,
                CategoryId = category.Id,
                BrandId = brand.Id,
                AverageRating = 4.8,
                ReviewCount = 1250,
                SoldCount = 5000
            };
            context.Products.Add(product);

            context.ProductImages.Add(new ProductImage
            {
                ImageUrl = "/images/products/iphone15-1.jpg",
                AltText = "iPhone 15 Pro Max Front",
                IsPrimary = true,
                DisplayOrder = 1,
                Product = product
            });

            var laptopCategory = await context.Categories.FirstOrDefaultAsync(c => c.Slug == "laptops");
            var dellBrand = await context.Brands.FirstOrDefaultAsync(b => b.Slug == "dell");

            if (laptopCategory != null && dellBrand != null)
            {
                var laptop = new Product
                {
                    Name = "Dell XPS 15",
                    Slug = "dell-xps-15",
                    ShortDescription = "Premium laptop with InfinityEdge display",
                    Description = "13th Gen Intel Core i7, 16GB DDR5 RAM, 512GB SSD, NVIDIA GeForce RTX 4050, 15.6-inch 3.5K OLED Display",
                    Price = 1899.99m,
                    SKU = "DEL-XPS15-2024",
                    StockQuantity = 50,
                    IsActive = true,
                    IsFeatured = true,
                    CategoryId = laptopCategory.Id,
                    BrandId = dellBrand.Id,
                    AverageRating = 4.6,
                    ReviewCount = 850,
                    SoldCount = 2000
                };
                context.Products.Add(laptop);
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedBannersAsync(ApplicationDbContext context)
    {
        if (await context.Banners.AnyAsync()) return;

        var banners = new List<Banner>
        {
            new() { Title = "Summer Sale", SubTitle = "Up to 50% off on electronics", ImageUrl = "/images/banners/summer-sale.jpg", ButtonText = "Shop Now", LinkUrl = "/products", IsActive = true, DisplayOrder = 1 },
            new() { Title = "New Arrivals", SubTitle = "Check out the latest products", ImageUrl = "/images/banners/new-arrivals.jpg", ButtonText = "Explore", LinkUrl = "/products?sort=newest", IsActive = true, DisplayOrder = 2 },
            new() { Title = "Free Shipping", SubTitle = "On orders over $50", ImageUrl = "/images/banners/free-shipping.jpg", ButtonText = "Learn More", LinkUrl = "/shipping-policy", IsActive = true, DisplayOrder = 3 }
        };

        context.Banners.AddRange(banners);
        await context.SaveChangesAsync();
    }

    private static async Task SeedSiteSettingsAsync(ApplicationDbContext context)
    {
        if (await context.SiteSettings.AnyAsync()) return;

        var settings = new List<SiteSetting>
        {
            new() { Key = "SiteName", Value = "ECommerce Website", Group = "General" },
            new() { Key = "SiteDescription", Value = "Your one-stop shop for everything", Group = "General" },
            new() { Key = "ContactEmail", Value = "contact@ecommerce.com", Group = "Contact" },
            new() { Key = "ContactPhone", Value = "+1-800-123-4567", Group = "Contact" },
            new() { Key = "Address", Value = "123 Commerce St, New York, NY 10001", Group = "Contact" },
            new() { Key = "FreeShippingThreshold", Value = "50", Group = "Shipping" },
            new() { Key = "StandardShippingCharge", Value = "5.99", Group = "Shipping" },
            new() { Key = "TaxRate", Value = "8.5", Group = "Tax" },
            new() { Key = "Currency", Value = "USD", Group = "General" },
            new() { Key = "CurrencySymbol", Value = "$", Group = "General" }
        };

        context.SiteSettings.AddRange(settings);
        await context.SaveChangesAsync();
    }

    private static async Task SeedCouponsAsync(ApplicationDbContext context)
    {
        if (await context.Coupons.AnyAsync()) return;

        var coupons = new List<Coupon>
        {
            new() { Code = "WELCOME10", Description = "10% off for new customers", DiscountType = DiscountType.Percentage, DiscountValue = 10, MinimumOrderAmount = 0, MaximumDiscount = 50, UsageLimit = 1000, IsActive = true, ValidFrom = DateTime.UtcNow, ValidTo = DateTime.UtcNow.AddYears(1) },
            new() { Code = "SAVE50", Description = "$50 off on orders over $200", DiscountType = DiscountType.FixedAmount, DiscountValue = 50, MinimumOrderAmount = 200, UsageLimit = 500, IsActive = true, ValidFrom = DateTime.UtcNow, ValidTo = DateTime.UtcNow.AddMonths(3) },
            new() { Code = "FREESHIP", Description = "Free shipping on orders over $50", DiscountType = DiscountType.FixedAmount, DiscountValue = 5.99m, MinimumOrderAmount = 50, UsageLimit = null, IsActive = true, ValidFrom = DateTime.UtcNow, ValidTo = DateTime.UtcNow.AddMonths(6) }
        };

        context.Coupons.AddRange(coupons);
        await context.SaveChangesAsync();
    }

    private static async Task SeedCmsPagesAsync(ApplicationDbContext context)
    {
        if (await context.CmsPages.AnyAsync()) return;

        var pages = new List<CmsPage>
        {
            new() { Title = "About Us", Slug = "about-us", Content = "<h2>About Our Store</h2><p>We are a leading e-commerce platform dedicated to providing the best shopping experience.</p>", IsActive = true, DisplayOrder = 1 },
            new() { Title = "Privacy Policy", Slug = "privacy-policy", Content = "<h2>Privacy Policy</h2><p>Your privacy is important to us.</p>", IsActive = true, DisplayOrder = 2 },
            new() { Title = "Terms of Service", Slug = "terms-of-service", Content = "<h2>Terms of Service</h2><p>Please read these terms carefully.</p>", IsActive = true, DisplayOrder = 3 },
            new() { Title = "Return Policy", Slug = "return-policy", Content = "<h2>Return Policy</h2><p>You may return items within 30 days.</p>", IsActive = true, DisplayOrder = 4 },
            new() { Title = "Shipping Policy", Slug = "shipping-policy", Content = "<h2>Shipping Policy</h2><p>We offer free shipping on orders over $50.</p>", IsActive = true, DisplayOrder = 5 },
            new() { Title = "FAQ", Slug = "faq", Content = "<h2>Frequently Asked Questions</h2><p>Find answers to common questions.</p>", IsActive = true, DisplayOrder = 6 }
        };

        context.CmsPages.AddRange(pages);
        await context.SaveChangesAsync();
    }
}
