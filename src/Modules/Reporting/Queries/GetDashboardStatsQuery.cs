using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Reporting.DTOs;

namespace Reporting.Queries;

public record GetDashboardStatsQuery : IRequest<DashboardStatsDto>;

public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, DashboardStatsDto>
{
    private readonly IUnitOfWork _unitOfWork;
    public GetDashboardStatsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
    public async Task<DashboardStatsDto> Handle(GetDashboardStatsQuery request, CancellationToken ct)
    {
        var today = DateTime.UtcNow.Date;
        var orders = _unitOfWork.Repository<Order>().GetQueryable();
        var products = _unitOfWork.Repository<Product>().GetQueryable();
        var inventory = _unitOfWork.Repository<Domain.Entities.Inventory>().GetQueryable();

        var totalRevenue = await orders.Where(o => o.Status == OrderStatus.Delivered).SumAsync(o => (decimal?)o.GrandTotal, ct) ?? 0;
        var totalOrders = await orders.CountAsync(ct);
        var totalProducts = await products.CountAsync(ct);
        var pendingOrders = await orders.CountAsync(o => o.Status == OrderStatus.Pending, ct);
        var lowStockItems = await inventory.CountAsync(i => (i.Quantity - i.ReservedQuantity) <= 5, ct);
        var revenueToday = await orders.Where(o => o.Status == OrderStatus.Delivered && o.CreatedAt >= today).SumAsync(o => (decimal?)o.GrandTotal, ct) ?? 0;
        var ordersToday = await orders.CountAsync(o => o.CreatedAt >= today, ct);

        var recentSales = await orders.Where(o => o.Status == OrderStatus.Delivered && o.CreatedAt >= today.AddDays(-7))
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new DailySalesDto { Date = g.Key, SalesAmount = g.Sum(o => o.GrandTotal), OrderCount = g.Count() })
            .OrderBy(d => d.Date).ToListAsync(ct);

        return new DashboardStatsDto
        {
            TotalRevenue = totalRevenue, TotalOrders = totalOrders, TotalProducts = totalProducts,
            PendingOrders = pendingOrders, LowStockItems = lowStockItems,
            RevenueToday = revenueToday, OrdersToday = ordersToday, RecentSales = recentSales
        };
    }
}
