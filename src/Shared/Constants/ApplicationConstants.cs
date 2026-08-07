namespace Shared.Constants;

public static class ApplicationConstants
{
    public const string ApplicationName = "ECommerceWebsite";
    public const string ApplicationVersion = "1.0.0";
    public const string DefaultConnectionStringName = "DefaultConnection";
    public const string AdminRole = "Admin";
    public const string CustomerRole = "Customer";
    public const string SellerRole = "Seller";
    public const string SuperAdminRole = "SuperAdmin";
    public const string DefaultPassword = "Password@123";
    public const int DefaultPageSize = 12;
    public const int MaxPageSize = 50;

    public static class CacheKeys
    {
        public const string AllCategories = "all_categories";
        public const string AllBrands = "all_brands";
        public const string FeaturedProducts = "featured_products";
        public const string BestSellers = "best_sellers";
        public const string TrendingProducts = "trending_products";
        public const string SiteSettings = "site_settings";
    }

    public static class SessionKeys
    {
        public const string Cart = "cart";
        public const string WishlistCount = "wishlist_count";
        public const string CartCount = "cart_count";
        public const string RecentProducts = "recent_products";
    }
}
