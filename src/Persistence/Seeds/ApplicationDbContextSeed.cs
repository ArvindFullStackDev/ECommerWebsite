using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;
using Shared.Constants;

namespace Persistence.Seeds;

public class ApplicationDbContextSeed
{
    public static async Task EnsureSchemaAsync(ApplicationDbContext context)
    {
        await EnsureColumnAsync(context, "Orders", "ConfirmedAt", "datetime2 NULL");
    }

    private static async Task EnsureLocalProductImagesAsync(ApplicationDbContext context, string? webRootPath)
    {
        if (string.IsNullOrWhiteSpace(webRootPath)) return;

        var images = await context.ProductImages
            .Where(i => i.ImageUrl != null && (i.ImageUrl.StartsWith("https://via.placeholder.com") || i.ImageUrl.StartsWith("/images/")))
            .Include(i => i.Product)
            .ToListAsync();

        var productsDir = Path.Combine(webRootPath, "images", "products");
        Directory.CreateDirectory(productsDir);

        foreach (var img in images)
        {
            var slug = string.IsNullOrWhiteSpace(img.Product?.Slug) ? "product" : img.Product.Slug;
            var fileName = slug + ".svg";
            var filePath = Path.Combine(productsDir, fileName);

            if (!File.Exists(filePath))
            {
                var name = img.Product?.Name ?? "Product";
                var escaped = System.Security.SecurityElement.Escape(name);
                var svg = $"<svg xmlns='http://www.w3.org/2000/svg' width='400' height='400'><rect width='400' height='400' fill='#232f3e'/><text x='50%' y='50%' fill='#ffffff' font-family='Arial, sans-serif' font-size='24' font-weight='bold' text-anchor='middle' dominant-baseline='middle'>{escaped}</text></svg>";
                await File.WriteAllTextAsync(filePath, svg);
            }

            img.ImageUrl = "/images/products/" + fileName;
        }

        await context.SaveChangesAsync();
    }

    private static async Task EnsureColumnAsync(ApplicationDbContext context, string table, string column, string definition)
    {
        var conn = context.Database.GetDbConnection();
        var openHere = conn.State != System.Data.ConnectionState.Open;
        if (openHere) await conn.OpenAsync();

        try
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(1) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @t AND COLUMN_NAME = @c";
            var pT = cmd.CreateParameter();
            pT.ParameterName = "@t";
            pT.Value = table;
            var pC = cmd.CreateParameter();
            pC.ParameterName = "@c";
            pC.Value = column;
            cmd.Parameters.Add(pT);
            cmd.Parameters.Add(pC);

            var count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            if (count == 0)
            {
                cmd.CommandText = $"ALTER TABLE [{table}] ADD [{column}] {definition}";
                await cmd.ExecuteNonQueryAsync();
            }
        }
        finally
        {
            if (openHere) await conn.CloseAsync();
        }
    }

    public static async Task SeedAsync(ApplicationDbContext context, UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager, string? webRootPath = null)
    {
        await SeedRolesAsync(roleManager);
        context.ChangeTracker.Clear();
        await SeedUsersAsync(userManager);
        context.ChangeTracker.Clear();
        await EnsureLocalProductImagesAsync(context, webRootPath);
        context.ChangeTracker.Clear();
        await SeedCategoriesAsync(context);
        context.ChangeTracker.Clear();
        await SeedBrandsAsync(context);
        context.ChangeTracker.Clear();
        await SeedProductsAsync(context, webRootPath);
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

    private static async Task SeedProductsAsync(ApplicationDbContext context, string? webRootPath = null)
    {
        var existingSkus = await context.Products.Where(p => p.SKU != null).Select(p => p.SKU).ToListAsync();

        var cat = await context.Categories.ToDictionaryAsync(c => c.Slug, c => c);
        var brand = await context.Brands.ToDictionaryAsync(b => b.Slug, b => b);

        var products = new List<Product>
        {
            // --- Mobile Phones ---
            new()
            {
                Name = "iPhone 15 Pro Max", Slug = "iphone-15-pro-max",
                ShortDescription = "The most powerful iPhone ever with A17 Pro chip",
                Description = "Experience the power of A17 Pro chip with 6-core GPU. Pro camera system with 48MP main camera, Ultra Wide, and Telephoto. Super Retina XDR display with ProMotion. All-day battery life.",
                Price = 101999.15m, DiscountPrice = 93499.15m, DiscountType = DiscountType.FixedAmount, DiscountValue = 8500,
                SKU = "APP-IP15PM-256", Barcode = "1234567890123", StockQuantity = 100, LowStockThreshold = 10,
                IsActive = true, IsFeatured = true, IsTrending = true, IsBestSeller = true,
                CategoryId = cat["mobile-phones"].Id, BrandId = brand["apple"].Id,
                AverageRating = 4.8, ReviewCount = 1250, SoldCount = 5000
            },
            new()
            {
                Name = "Samsung Galaxy S24 Ultra", Slug = "samsung-galaxy-s24-ultra",
                ShortDescription = "Galaxy AI is here with Titanium design",
                Description = "Titanium frame, 200MP camera, built-in S Pen, and Galaxy AI features that unlock a new level of intelligence.",
                Price = 110499.15m, DiscountPrice = 97749.15m, DiscountType = DiscountType.FixedAmount, DiscountValue = 12750,
                SKU = "SAM-S24U-512", Barcode = "2345678901234", StockQuantity = 80, LowStockThreshold = 10,
                IsActive = true, IsFeatured = true, IsBestSeller = true,
                CategoryId = cat["mobile-phones"].Id, BrandId = brand["samsung"].Id,
                AverageRating = 4.7, ReviewCount = 980, SoldCount = 3200
            },
            // --- Laptops ---
            new()
            {
                Name = "Dell XPS 15", Slug = "dell-xps-15",
                ShortDescription = "Premium laptop with InfinityEdge display",
                Description = "13th Gen Intel Core i7, 16GB DDR5 RAM, 512GB SSD, NVIDIA GeForce RTX 4050, 15.6-inch 3.5K OLED Display",
                Price = 161499.15m, SKU = "DEL-XPS15-2024",
                StockQuantity = 50, LowStockThreshold = 5,
                IsActive = true, IsFeatured = true,
                CategoryId = cat["laptops"].Id, BrandId = brand["dell"].Id,
                AverageRating = 4.6, ReviewCount = 850, SoldCount = 2000
            },
            new()
            {
                Name = "HP Spectre x360 14", Slug = "hp-spectre-x360-14",
                ShortDescription = "2-in-1 convertible with OLED touch display",
                Description = "Intel Core Ultra 7, 16GB RAM, 1TB SSD, 14-inch 2.8K OLED touch display with 360-degree hinge.",
                Price = 118999.15m, DiscountPrice = 106249.15m, DiscountType = DiscountType.FixedAmount, DiscountValue = 12750,
                SKU = "HP-SPX-14-2024", StockQuantity = 40, LowStockThreshold = 5,
                IsActive = true, IsFeatured = true,
                CategoryId = cat["laptops"].Id, BrandId = brand["hp"].Id,
                AverageRating = 4.5, ReviewCount = 620, SoldCount = 1500
            },
            // --- Headphones ---
            new()
            {
                Name = "Sony WH-1000XM5", Slug = "sony-wh-1000xm5",
                ShortDescription = "Industry-leading noise cancellation",
                Description = "Best-in-class noise cancellation, exceptional sound quality, 30-hour battery life, and multipoint connection.",
                Price = 33999.15m, DiscountPrice = 28049.15m, DiscountType = DiscountType.FixedAmount, DiscountValue = 5950,
                SKU = "SON-WH1000XM5", StockQuantity = 120, LowStockThreshold = 15,
                IsActive = true, IsFeatured = true, IsBestSeller = true, IsTrending = true,
                CategoryId = cat["headphones"].Id, BrandId = brand["sony"].Id,
                AverageRating = 4.9, ReviewCount = 2100, SoldCount = 8500
            },
            // --- Electronics (direct) ---
            new()
            {
                Name = "Samsung 65\" Crystal UHD TV", Slug = "samsung-65-crystal-uhd-tv",
                ShortDescription = "Stunning 4K picture with Crystal Processor",
                Description = "65-inch Crystal UHD 4K smart TV with Crystal Processor 4K, HDR10+, and built-in voice assistant support.",
                Price = 67999.15m, DiscountPrice = 55249.15m, DiscountType = DiscountType.FixedAmount, DiscountValue = 12750,
                SKU = "SAM-TV65-CUHD", StockQuantity = 30, LowStockThreshold = 5,
                IsActive = true, IsTrending = true,
                CategoryId = cat["electronics"].Id, BrandId = brand["samsung"].Id,
                AverageRating = 4.6, ReviewCount = 720, SoldCount = 1800
            },
            // --- Clothing ---
            new()
            {
                Name = "Nike Dri-FIT T-Shirt", Slug = "nike-dri-fit-tshirt",
                ShortDescription = "Breathable performance t-shirt",
                Description = "Nike Dri-FIT technology moves sweat away from your skin for quicker evaporation, helping you stay dry and comfortable.",
                Price = 2124.15m, DiscountPrice = 1699.15m, DiscountType = DiscountType.Percentage, DiscountValue = 20,
                SKU = "NK-DF-TSHIRT", StockQuantity = 500, LowStockThreshold = 50,
                IsActive = true, IsBestSeller = true,
                CategoryId = cat["mens-clothing"].Id, BrandId = brand["nike"].Id,
                AverageRating = 4.7, ReviewCount = 3400, SoldCount = 12000
            },
            new()
            {
                Name = "Adidas Essentials Hoodie", Slug = "adidas-essentials-hoodie",
                ShortDescription = "Classic pullover hoodie with fleece lining",
                Description = "Soft cotton-blend fleece hoodie with adjustable hood, kangaroo pocket, and ribbed cuffs.",
                Price = 5099.15m, DiscountPrice = 3824.15m, DiscountType = DiscountType.Percentage, DiscountValue = 25,
                SKU = "ADI-ESS-HOODIE", StockQuantity = 300, LowStockThreshold = 30,
                IsActive = true, IsFeatured = true,
                CategoryId = cat["womens-clothing"].Id, BrandId = brand["adidas"].Id,
                AverageRating = 4.5, ReviewCount = 1200, SoldCount = 4500
            },
            new()
            {
                Name = "Nike Sportswear Tech Fleece Joggers", Slug = "nike-tech-fleece-joggers",
                ShortDescription = "Premium tech fleece joggers",
                Description = "Sleek, streamlined joggers with a premium look and feel, made with recycled materials.",
                Price = 7649.15m, SKU = "NK-TF-JOGGERS", StockQuantity = 250, LowStockThreshold = 25,
                IsActive = true, IsTrending = true,
                CategoryId = cat["mens-clothing"].Id, BrandId = brand["nike"].Id,
                AverageRating = 4.6, ReviewCount = 890, SoldCount = 3100
            },
            // --- Home & Garden ---
            new()
            {
                Name = "Instant Pot Duo 7-in-1", Slug = "instant-pot-duo-7-in-1",
                ShortDescription = "Pressure cooker, slow cooker, rice cooker & more",
                Description = "The 7-in-1 multi-cooker that replaces seven appliances: pressure cooker, slow cooker, rice cooker, steamer, saute, yogurt maker, and warmer.",
                Price = 8499.15m, DiscountPrice = 6799.15m, DiscountType = DiscountType.Percentage, DiscountValue = 20,
                SKU = "IP-DUO-6QT", Barcode = "3456789012345", StockQuantity = 200, LowStockThreshold = 20,
                IsActive = true, IsFeatured = true, IsBestSeller = true,
                CategoryId = cat["home-garden"].Id,
                AverageRating = 4.8, ReviewCount = 9800, SoldCount = 45000
            },
            new()
            {
                Name = "Breville Barista Express", Slug = "breville-barista-express",
                ShortDescription = "Semi-automatic espresso machine",
                Description = "Brushed stainless steel espresso machine with built-in conical burr grinder, 15-bar pump, and micro-foam milk texturing.",
                Price = 63749.15m, DiscountPrice = 55249.15m, DiscountType = DiscountType.FixedAmount, DiscountValue = 8500,
                SKU = "BRV-BES870", StockQuantity = 45, LowStockThreshold = 5,
                IsActive = true, IsTrending = true,
                CategoryId = cat["home-garden"].Id,
                AverageRating = 4.7, ReviewCount = 3100, SoldCount = 7500
            },
            new()
            {
                Name = "Cozy Comfort Throw Blanket", Slug = "cozy-comfort-throw-blanket",
                ShortDescription = "Ultra-soft faux fur throw blanket",
                Description = "Luxuriously soft 50x60-inch throw blanket in a modern grey color. Perfect for the couch, bed, or travel.",
                Price = 3399.15m, SKU = "COZ-THROW-GRY", StockQuantity = 400, LowStockThreshold = 40,
                IsActive = true, IsFeatured = true,
                CategoryId = cat["home-garden"].Id,
                AverageRating = 4.4, ReviewCount = 1500, SoldCount = 5200
            },
            // --- Books ---
            new()
            {
                Name = "Clean Code", Slug = "clean-code",
                ShortDescription = "A Handbook of Agile Software Craftsmanship",
                Description = "Best-selling guide to writing clean, maintainable code by Robert C. Martin. A must-read for every developer.",
                Price = 2974.15m, DiscountPrice = 2379.15m, DiscountType = DiscountType.Percentage, DiscountValue = 20,
                SKU = "BK-CLEANCODE", Barcode = "4567890123456", StockQuantity = 350, LowStockThreshold = 35,
                IsActive = true, IsBestSeller = true,
                CategoryId = cat["books"].Id,
                AverageRating = 4.7, ReviewCount = 4200, SoldCount = 18000
            },
            new()
            {
                Name = "C# in Depth, 5th Edition", Slug = "c-sharp-in-depth-5th",
                ShortDescription = "Master the modern C# language",
                Description = "Jon Skeet's definitive guide to the C# language, covering everything from generics to the latest C# features.",
                Price = 4249.15m, DiscountPrice = 3399.15m, DiscountType = DiscountType.Percentage, DiscountValue = 20,
                SKU = "BK-CSINDEPTH5", StockQuantity = 150, LowStockThreshold = 15,
                IsActive = true, IsFeatured = true,
                CategoryId = cat["books"].Id,
                AverageRating = 4.8, ReviewCount = 900, SoldCount = 3800
            },
            new()
            {
                Name = "Atomic Habits", Slug = "atomic-habits",
                ShortDescription = "Tiny Changes, Remarkable Results",
                Description = "James Clear's proven framework for building good habits and breaking bad ones through tiny, incremental changes.",
                Price = 1699.15m, SKU = "BK-ATOMIC", Barcode = "5678901234567", StockQuantity = 600, LowStockThreshold = 60,
                IsActive = true, IsBestSeller = true, IsTrending = true,
                CategoryId = cat["books"].Id,
                AverageRating = 4.9, ReviewCount = 15000, SoldCount = 65000
            },
            // --- Sports & Outdoors ---
            new()
            {
                Name = "Nike Air Zoom Pegasus 40", Slug = "nike-air-zoom-pegasus-40",
                ShortDescription = "Everyday running shoes with responsive cushioning",
                Description = "Trusted everyday running shoe with Zoom Air cushioning and a breathable engineered mesh upper for all-day comfort.",
                Price = 11049.15m, DiscountPrice = 8499.15m, DiscountType = DiscountType.Percentage, DiscountValue = 23,
                SKU = "NK-PEG40-01", StockQuantity = 180, LowStockThreshold = 20,
                IsActive = true, IsFeatured = true, IsBestSeller = true,
                CategoryId = cat["sports-outdoors"].Id, BrandId = brand["nike"].Id,
                AverageRating = 4.8, ReviewCount = 6800, SoldCount = 28000
            },
            new()
            {
                Name = "Adidas Ultraboost Light", Slug = "adidas-ultraboost-light",
                ShortDescription = "Lightest Ultraboost ever",
                Description = "Boost energy return with a lighter, breathable design. Engineered for runners who want responsiveness and comfort.",
                Price = 16149.15m, DiscountPrice = 13174.15m, DiscountType = DiscountType.Percentage, DiscountValue = 18,
                SKU = "ADI-UB-LIGHT", StockQuantity = 140, LowStockThreshold = 15,
                IsActive = true, IsTrending = true,
                CategoryId = cat["sports-outdoors"].Id, BrandId = brand["adidas"].Id,
                AverageRating = 4.6, ReviewCount = 2100, SoldCount = 8700
            },
            new()
            {
                Name = "Pro Fitness Yoga Mat", Slug = "pro-fitness-yoga-mat",
                ShortDescription = "Non-slip exercise mat with carrying strap",
                Description = "Extra-thick 6mm non-slip yoga and exercise mat made from eco-friendly TPE. Includes carrying strap.",
                Price = 2549.15m, SKU = "PF-YOGA-MAT", StockQuantity = 700, LowStockThreshold = 70,
                IsActive = true, IsFeatured = true,
                CategoryId = cat["sports-outdoors"].Id,
                AverageRating = 4.5, ReviewCount = 5200, SoldCount = 19000
            }
        };

        foreach (var product in products.Where(p => !existingSkus.Contains(p.SKU)))
        {
            var localImageUrl = "/images/products/product.svg";
            if (!string.IsNullOrWhiteSpace(webRootPath))
            {
                var productsDir = Path.Combine(webRootPath, "images", "products");
                Directory.CreateDirectory(productsDir);
                var fileName = product.Slug + ".svg";
                var filePath = Path.Combine(productsDir, fileName);
                if (!File.Exists(filePath))
                {
                    var escaped = System.Security.SecurityElement.Escape(product.Name);
                    var svg = $"<svg xmlns='http://www.w3.org/2000/svg' width='400' height='400'><rect width='400' height='400' fill='#232f3e'/><text x='50%' y='50%' fill='#ffffff' font-family='Arial, sans-serif' font-size='24' font-weight='bold' text-anchor='middle' dominant-baseline='middle'>{escaped}</text></svg>";
                    File.WriteAllText(filePath, svg);
                }
                localImageUrl = "/images/products/" + fileName;
            }

            context.Products.Add(product);
            context.ProductImages.Add(new ProductImage
            {
                ImageUrl = localImageUrl,
                AltText = product.Name,
                IsPrimary = true,
                DisplayOrder = 1,
                Product = product
            });
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
            new() { Title = "Free Shipping", SubTitle = "On orders over ₹4,250", ImageUrl = "/images/banners/free-shipping.jpg", ButtonText = "Learn More", LinkUrl = "/shipping-policy", IsActive = true, DisplayOrder = 3 }
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
            new() { Key = "Address", Value = "123 Commerce St, Mumbai, Virar, Maharashtra 401303", Group = "Contact" },
            new() { Key = "FreeShippingThreshold", Value = "4250", Group = "Shipping" },
            new() { Key = "StandardShippingCharge", Value = "509.15", Group = "Shipping" },
            new() { Key = "TaxRate", Value = "8.5", Group = "Tax" },
            new() { Key = "Currency", Value = "INR", Group = "General" },
            new() { Key = "CurrencySymbol", Value = "₹", Group = "General" }
        };

        context.SiteSettings.AddRange(settings);
        await context.SaveChangesAsync();
    }

    private static async Task SeedCouponsAsync(ApplicationDbContext context)
    {
        if (await context.Coupons.AnyAsync()) return;

        var coupons = new List<Coupon>
        {
            new() { Code = "WELCOME10", Description = "10% off for new customers", DiscountType = DiscountType.Percentage, DiscountValue = 10, MinimumOrderAmount = 0, MaximumDiscount = 4250, UsageLimit = 1000, IsActive = true, ValidFrom = DateTime.UtcNow, ValidTo = DateTime.UtcNow.AddYears(1) },
            new() { Code = "SAVE50", Description = "₹4,250 off on orders over ₹17,000", DiscountType = DiscountType.FixedAmount, DiscountValue = 4250, MinimumOrderAmount = 17000, UsageLimit = 500, IsActive = true, ValidFrom = DateTime.UtcNow, ValidTo = DateTime.UtcNow.AddMonths(3) },
            new() { Code = "FREESHIP", Description = "Free shipping on orders over ₹4,250", DiscountType = DiscountType.FixedAmount, DiscountValue = 509.15m, MinimumOrderAmount = 4250, UsageLimit = null, IsActive = true, ValidFrom = DateTime.UtcNow, ValidTo = DateTime.UtcNow.AddMonths(6) }
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
            new() { Title = "Shipping Policy", Slug = "shipping-policy", Content = "<h2>Shipping Policy</h2><p>We offer free shipping on orders over ₹4,250.</p>", IsActive = true, DisplayOrder = 5 },
            new() { Title = "FAQ", Slug = "faq", Content = "<h2>Frequently Asked Questions</h2><p>Find answers to common questions.</p>", IsActive = true, DisplayOrder = 6 }
        };

        context.CmsPages.AddRange(pages);
        await context.SaveChangesAsync();
    }
}
