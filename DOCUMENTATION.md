# ECommerce Website - Enterprise E-Commerce Platform

## Architecture Overview

A modular monolith ASP.NET Core MVC e-commerce platform built with Clean Architecture, CQRS, and Domain-Driven Design principles — inspired by Amazon.

## Technology Stack

| Layer | Technology |
|-------|-----------|
| Runtime | .NET 10 |
| Framework | ASP.NET Core MVC |
| ORM | Entity Framework Core 10 |
| Database | SQL Server (LocalDB / SQL Server) |
| UI | Bootstrap 5.3+, Font Awesome 6, Bootstrap Icons |
| CQRS | MediatR 12 |
| Mapping | AutoMapper 12 |
| Validation | FluentValidation 11 |
| Logging | Serilog |
| Auth | ASP.NET Core Identity + JWT |
| Real-time | SignalR |
| Caching | IMemoryCache + IDistributedCache |

## Solution Structure (17 Projects)

```
ECommerceWebsite/
├── src/
│   ├── Domain/           # Entities, enums, interfaces, value objects
│   ├── Shared/           # Constants, models (ApiResponse, PagedResult), helpers
│   ├── Application/      # CQRS pipeline, common interfaces, DI registration
│   ├── Persistence/      # EF Core DbContext, Fluent API configs, seed data
│   ├── Infrastructure/   # JWT service, cache, current user, repositories, UoW
│   ├── Modules/          # Feature modules (one per bounded context)
│   │   ├── Identity/     # Register, Login, ForgotPassword, ResetPassword
│   │   ├── Catalog/      # Categories, Brands, Products (full CRUD + search)
│   │   ├── Cart/         # Shopping cart management
│   │   ├── Wishlist/     # Wishlist management
│   │   ├── Orders/       # Order processing lifecycle
│   │   ├── Payments/     # Payment gateway integration
│   │   ├── Inventory/    # Stock management, history, low-stock alerts
│   │   ├── Reviews/      # Ratings, comments, images, likes
│   │   ├── Notification/ # Email, SMS, push, in-app notifications
│   │   ├── Reporting/    # Sales, customer, product, revenue reports
│   │   └── Admin/        # Admin dashboard services
│   └── Presentation/
│       └── WebUI/        # MVC controllers, Razor views, static assets
```

## Module Architecture (CQRS Pattern)

Every feature follows this structure:

```
Module/
├── Commands/
│   ├── CreateX/
│   │   ├── CreateXCommand.cs
│   │   ├── CreateXCommandHandler.cs
│   │   └── CreateXCommandValidator.cs
│   └── UpdateX/
│       ├── UpdateXCommand.cs
│       ├── UpdateXCommandHandler.cs
│       └── UpdateXCommandValidator.cs
├── Queries/
│   ├── GetXQuery.cs
│   └── GetXQueryHandler.cs
├── DTOs/
├── Profiles/  (AutoMapper)
├── Validators/
├── Interfaces/
├── Services/
└── Extensions/
```

## Installation

### Prerequisites
- .NET 10 SDK
- SQL Server (LocalDB included with Visual Studio, or any SQL Server instance)

### Steps

```powershell
# Clone or navigate to the project
cd ECommerceWebsite

# Restore packages
dotnet restore

# Update connection string (optional - defaults to LocalDB)
# Edit: src/Presentation/WebUI/appsettings.json

# Run the application (creates DB + seeds data automatically)
cd src/Presentation/WebUI
dotnet run
```

The application listens on `http://localhost:5000` by default.

### Using EF Core Migrations (Production)

```powershell
# Install EF Core tools if not installed
dotnet tool install --global dotnet-ef

# Create initial migration
dotnet ef migrations add InitialCreate --project src\Persistence --startup-project src\Presentation\WebUI

# Apply migration
dotnet ef database update --project src\Persistence --startup-project src\Presentation\WebUI

# Remove EnsureCreated from Program.cs after first migration
```

## Seed Data

On first run, the application seeds:

| Data | Details |
|------|---------|
| Roles | SuperAdmin, Admin, Customer, Seller |
| Users | admin@ecommerce.com / Admin@123, customer@ecommerce.com / Customer@123 |
| Categories | Electronics, Clothing, Home & Garden, Books, Sports (with subcategories) |
| Brands | Apple, Samsung, Sony, Nike, Adidas, Microsoft, Dell, HP |
| Products | iPhone 15 Pro Max, Dell XPS 15 (with images) |
| Banners | 3 hero banners |
| Coupons | WELCOME10 (10% off), SAVE50 ($50 off), FREESHIP (free shipping) |
| CMS Pages | About Us, Privacy Policy, Terms, Return Policy, Shipping Policy, FAQ |
| Site Settings | Site name, contact info, shipping thresholds, tax rates |

## Database Schema

### Core Entities
- **Category** — Hierarchical categories (self-referencing FK)
- **Brand** — Product brands
- **Product** — Full product catalog with pricing, discounts, SEO
- **ProductImage** — Multiple images per product
- **ProductVariant** — Size/color/configuration variants
- **ProductTag** — Searchable tags
- **ProductQuestion** — Customer Q&A

### Cart & Wishlist
- **Cart** — One cart per user
- **CartItem** — Line items with quantity, pricing
- **Wishlist** — Saved products

### Orders
- **Order** — Full order lifecycle (Pending → Confirmed → Packed → Shipped → Delivered)
- **OrderItem** — Order line items
- **Address** — Shipping/billing addresses

### Payments
- **Payment** — Transaction records with gateway response
- **Coupon** — Discount codes with usage limits

### Reviews
- **Review** — Ratings (1-5), comments, images, videos
- **ReviewLike** — Like/unlike reviews

### Inventory
- **Inventory** — Stock levels per product
- **StockHistory** — Audit trail of stock changes
- **Supplier** — Vendor management

### Other
- **Notification** — User notifications (in-app)
- **Banner** — Promotional banners
- **CmsPage** — Static content pages
- **SiteSetting** — Key-value configuration store
- **Wallet** — User wallet balance
- **WalletTransaction** — Wallet transaction history
- **RecentlyViewed** — User browsing history
- **CompareProduct** — Product comparison list

## Authentication & Authorization

### Features
- Register with email/password
- Login with JWT token + cookie
- Forgot / Reset password
- Role-based authorization (Customer, Seller, Admin, SuperAdmin)

### Identity Configuration
```csharp
services.AddIdentity<IdentityUser, IdentityRole>(options => {
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.User.RequireUniqueEmail = true;
});
```

### JWT Settings (appsettings.json)
```json
"JwtSettings": {
    "SecretKey": "your-256-bit-secret-key-here",
    "Issuer": "ECommerceWebsite",
    "Audience": "ECommerceWebsiteUsers",
    "ExpirationMinutes": 60
}
```

## API Endpoints (Controllers)

### Public
| Route | Action | Description |
|-------|--------|-------------|
| `/` | Home.Index | Home page with featured, bestseller, trending products |
| `/Catalog` | Catalog.Index | Product listing with filters |
| `/Catalog/Search` | Catalog.Search | Search products |
| `/Catalog/Details/{id}` | Catalog.Details | Product detail page |
| `/Catalog/Deals` | Catalog.Deals | Today's deals |
| `/Catalog/BestSellers` | Catalog.BestSellers | Best sellers |

### Authentication
| Route | Action | Description |
|-------|--------|-------------|
| `/Account/Login` | Account.Login | Login form |
| `/Account/Register` | Account.Register | Registration form |
| `/Account/Logout` | Account.Logout | Logout |
| `/Account/ForgotPassword` | Account.ForgotPassword | Password reset request |
| `/Account/ResetPassword` | Account.ResetPassword | Reset password |

### Authenticated
| Route | Action | Description |
|-------|--------|-------------|
| `/Cart` | Cart.Index | View cart |
| `/Cart/AddToCart` | Cart.AddToCart | Add item (AJAX) |
| `/Cart/UpdateQuantity` | Cart.UpdateQuantity | Update qty (AJAX) |
| `/Cart/RemoveFromCart` | Cart.RemoveFromCart | Remove item (AJAX) |
| `/Wishlist` | Wishlist.Index | View wishlist |
| `/Wishlist/AddToWishlist` | Wishlist.AddToWishlist | Add item (AJAX) |

### Admin (Area: Admin, Role: Admin/SuperAdmin)
| Route | Action | Description |
|-------|--------|-------------|
| `/Admin/Dashboard` | Dashboard.Index | Admin dashboard with stats |
| `/Admin/Products` | Products.Index | Product CRUD |
| `/Admin/Categories` | Categories.Index | Category CRUD |
| `/Admin/Orders` | Orders.Index | Order management |

## UI Features (Amazon-Inspired)

### Frontend
- Top navbar with logo, location, search, account menu, cart, wishlist
- Mega menu category navigation
- Hero carousel slider (3 slides)
- Product cards with star ratings, discounts, stock badges
- Product detail page with image gallery, quantity selector, buy now
- Shopping cart with quantity adjustment, save for later
- Wishlist with move-to-cart functionality
- Filter sidebar (categories, brands, price range)
- Sorting (price, newest, bestseller, rating)
- Pagination
- Breadcrumb navigation
- Flash sale countdown timers
- Toast notifications (AJAX add-to-cart)
- Skeleton loader animations
- Professional footer with links
- Fully responsive (mobile, tablet, desktop)

### Admin Panel
- Sidebar navigation
- Stat cards (total products, orders, revenue, pending)
- Quick action buttons
- Low stock alerts
- Table layout for data management

## Configuration

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ECommerceWebsite;..."
  },
  "JwtSettings": {
    "SecretKey": "...",
    "Issuer": "ECommerceWebsite",
    "Audience": "ECommerceWebsiteUsers",
    "ExpirationMinutes": 60
  },
  "Serilog": {
    "MinimumLevel": { "Default": "Information" },
    "WriteTo": [
      { "Name": "Console" },
      { "Name": "File", "Args": { "path": "logs/log-.txt", "rollingInterval": "Day" } }
    ]
  }
}
```

## Project Dependencies

```
WebUI ──────────────────────────────┐
  ├── Application (CQRS + Behaviors)│
  ├── Domain (Entities)             │
  ├── Infrastructure (Services)     │
  │     └── Identity Module         │
  ├── Persistence (EF Core)         │
  ├── Shared (Common)               │
  ├── Identity Module               │
  ├── Catalog Module                │
  ├── Cart Module                   │
  ├── Wishlist Module               │
  ├── Orders Module                 │
  ├── Payments Module               │
  ├── Inventory Module              │
  ├── Reviews Module                │
  ├── Notification Module           │
  ├── Reporting Module              │
  └── Admin Module                  │
```

## Deployment

### Production Considerations
1. **Migrations**: Switch from `EnsureCreated()` to EF Core migrations
2. **Connection string**: Use SQL Server (not LocalDB)
3. **JWT Secret**: Use a strong, unique secret key stored in environment variables or Azure Key Vault
4. **HTTPS**: Enforce HTTPS with proper certificate
5. **Caching**: Configure Redis for distributed cache
6. **Logging**: Set up Serilog sinks (ElasticSearch, Seq, or Azure Application Insights)
7. **CDN**: Serve Bootstrap, Font Awesome from CDN or self-host
8. **Images**: Use blob storage (Azure Blob, AWS S3) for product images
9. **Session**: Configure SQL Server or Redis for distributed session state
10. **Error handling**: Custom error pages for 404, 500, maintenance mode

### Docker Support
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish src/Presentation/WebUI/WebUI.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "WebUI.dll"]
```

## Project Conventions

### Coding Standards
- All classes have XML documentation comments
- Interfaces are prefixed with `I`
- Async methods end with `Async`
- Commands/Queries are immutable records or plain objects
- Handlers are separated by concern (one handler per command/query)
- Validators use FluentValidation
- Controllers only call MediatR — no business logic
- Repositories return `IReadOnlyList<T>` for queries
- Soft delete is used instead of hard delete
- All monetary values use `decimal(18,2)` in SQL

### Error Handling
- `NotFoundException` → 404 responses
- `ValidationException` → 400 with validation errors
- `ApiResponse<T>` wraps all responses with Success/Message/Errors
- Serilog captures all unhandled exceptions
- `ExceptionHandler` middleware redirects to error page

## Caching Strategy

| Cache | Type | Duration | Usage |
|-------|------|----------|-------|
| Categories | Memory | 30 min | Navigation, filters |
| Brands | Memory | 30 min | Filters, brand pages |
| Featured Products | Memory | 10 min | Home page |
| Site Settings | Memory | 60 min | Site-wide config |
| Session | Distributed | 2 hours | Cart, temp data |

---

*Documentation generated for ECommerce Website v1.0.0*