using Admin.Extensions;
using Application.Extensions;
using Cart.Extensions;
using Catalog.Extensions;
using Identity.Extensions;
using Infrastructure.Extensions;
using Inventory.Extensions;
using Notification.Extensions;
using Notification.Hubs;
using Orders.Extensions;
using Payments.Extensions;
using Persistence.Extensions;
using Reporting.Extensions;
using Reviews.Extensions;
using Serilog;
using Wishlist.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration.ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();
});

// Add services
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();

builder.Services.AddRazorPages();

// Add layers
builder.Services.AddApplicationLayer();
builder.Services.AddPersistenceLayer(builder.Configuration);
builder.Services.AddInfrastructureLayer(builder.Configuration);

// Add modules
builder.Services.AddIdentityModule();
builder.Services.AddCatalogModule();
builder.Services.AddCartModule();
builder.Services.AddWishlistModule();
builder.Services.AddOrderModule();
builder.Services.AddPaymentModule();
builder.Services.AddReviewModule();
builder.Services.AddInventoryModule();
builder.Services.AddNotificationModule();
builder.Services.AddReportingModule();
builder.Services.AddAdminModule();

// Compression
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

var app = builder.Build();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<Persistence.Data.ApplicationDbContext>();
        context.Database.EnsureCreated();

        var userManager = services.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<Microsoft.AspNetCore.Identity.IdentityUser>>();
        var roleManager = services.GetRequiredService<Microsoft.AspNetCore.Identity.RoleManager<Microsoft.AspNetCore.Identity.IdentityRole>>();
        await Persistence.Seeds.ApplicationDbContextSeed.SeedAsync(context, userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseResponseCompression();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();
app.MapHub<NotificationHub>("/notificationHub");

app.Run();
