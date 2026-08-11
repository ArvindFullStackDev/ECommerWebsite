using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebUI.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class ReportsController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<IdentityUser> _userManager;

    public ReportsController(IUnitOfWork unitOfWork, UserManager<IdentityUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var orderQuery = _unitOfWork.Repository<Order>().GetQueryable();

        var totalRevenue = await orderQuery.Where(o => o.Status == OrderStatus.Delivered)
            .SumAsync(o => o.GrandTotal);
        var totalOrders = await orderQuery.CountAsync();
        var avgOrderValue = totalOrders > 0 ? await orderQuery.AverageAsync(o => o.GrandTotal) : 0;
        var customers = await _userManager.Users.CountAsync();

        var topProducts = await _unitOfWork.Repository<OrderItem>().GetQueryable()
            .Include(i => i.Product)
            .Where(i => i.Order.Status == OrderStatus.Delivered)
            .GroupBy(i => new { i.ProductId, Name = i.Product.Name })
            .Select(g => new
            {
                g.Key.ProductId,
                g.Key.Name,
                Quantity = g.Sum(i => i.Quantity),
                Revenue = g.Sum(i => i.TotalPrice)
            })
            .OrderByDescending(g => g.Quantity)
            .Take(5)
            .ToListAsync();

        var revenueByStatus = await orderQuery
            .GroupBy(o => o.Status)
            .Select(g => new { Status = g.Key, Orders = g.Count(), Revenue = g.Sum(o => o.GrandTotal) })
            .ToListAsync();

        var recentOrders = await orderQuery
            .OrderByDescending(o => o.Id)
            .Take(10)
            .ToListAsync();

        ViewBag.TotalRevenue = totalRevenue;
        ViewBag.TotalOrders = totalOrders;
        ViewBag.AvgOrderValue = avgOrderValue;
        ViewBag.TotalCustomers = customers;
        ViewBag.TopProducts = topProducts;
        ViewBag.RevenueByStatus = revenueByStatus;
        ViewBag.RecentOrders = recentOrders;

        return View();
    }
}
