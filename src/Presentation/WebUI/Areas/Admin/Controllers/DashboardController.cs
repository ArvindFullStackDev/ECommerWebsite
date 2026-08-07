using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebUI.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class DashboardController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public DashboardController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Index()
    {
        var totalProducts = await _unitOfWork.Repository<Product>().CountAsync();
        var totalOrders = await _unitOfWork.Repository<Order>().CountAsync();
        var totalCustomers = 0; // would need identity user count
        var totalRevenue = await _unitOfWork.Repository<Order>().GetQueryable()
            .Where(o => o.Status == Domain.Enums.OrderStatus.Delivered)
            .SumAsync(o => o.GrandTotal);
        var pendingOrders = await _unitOfWork.Repository<Order>()
            .CountAsync(o => o.Status == Domain.Enums.OrderStatus.Pending);
        var lowStockProducts = await _unitOfWork.Repository<Product>()
            .CountAsync(p => p.StockQuantity <= p.LowStockThreshold && p.IsActive);

        ViewBag.TotalProducts = totalProducts;
        ViewBag.TotalOrders = totalOrders;
        ViewBag.TotalRevenue = totalRevenue;
        ViewBag.PendingOrders = pendingOrders;
        ViewBag.LowStockProducts = lowStockProducts;

        return View();
    }
}
