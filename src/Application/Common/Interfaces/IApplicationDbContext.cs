using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Category> Categories { get; }
    DbSet<Brand> Brands { get; }
    DbSet<Product> Products { get; }
    DbSet<ProductImage> ProductImages { get; }
    DbSet<ProductVariant> ProductVariants { get; }
    DbSet<ProductTag> ProductTags { get; }
    DbSet<ProductQuestion> ProductQuestions { get; }
    DbSet<Review> Reviews { get; }
    DbSet<ReviewLike> ReviewLikes { get; }
    DbSet<Cart> Carts { get; }
    DbSet<CartItem> CartItems { get; }
    DbSet<Wishlist> Wishlists { get; }
    DbSet<Address> Addresses { get; }
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }
    DbSet<Payment> Payments { get; }
    DbSet<Coupon> Coupons { get; }
    DbSet<Inventory> Inventories { get; }
    DbSet<StockHistory> StockHistories { get; }
    DbSet<Supplier> Suppliers { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<Banner> Banners { get; }
    DbSet<CmsPage> CmsPages { get; }
    DbSet<SiteSetting> SiteSettings { get; }
    DbSet<RecentlyViewed> RecentlyVieweds { get; }
    DbSet<CompareProduct> CompareProducts { get; }
    DbSet<Wallet> Wallets { get; }
    DbSet<WalletTransaction> WalletTransactions { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
