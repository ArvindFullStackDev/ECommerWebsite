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
public class OrdersController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<IdentityUser> _userManager;

    public OrdersController(IUnitOfWork unitOfWork, UserManager<IdentityUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(OrderStatus? status)
    {
        IQueryable<Order> query = _unitOfWork.Repository<Order>().GetQueryable()
            .Include(o => o.Items);

        if (status.HasValue)
            query = query.Where(o => o.Status == status.Value);

        var orders = await query.OrderByDescending(o => o.Id).ToListAsync();

        var userIds = orders.Select(o => o.UserId).Distinct().ToList();
        var users = await _userManager.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.Email);

        ViewBag.Users = users;
        ViewBag.Status = status;
        ViewBag.Statuses = Enum.GetValues<OrderStatus>();
        return View(orders);
    }

    public async Task<IActionResult> Details(int id)
    {
        var order = await _unitOfWork.Repository<Order>().GetQueryable()
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Include(o => o.ShippingAddress)
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return NotFound();

        var user = await _userManager.FindByIdAsync(order.UserId);
        ViewBag.CustomerEmail = user?.Email ?? "Unknown";
        return View(order);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateStatus(int id, OrderStatus status)
    {
        var order = await _unitOfWork.Repository<Order>().GetByIdAsync(id);
        if (order == null) return NotFound();

        order.Status = status;
        if (status == OrderStatus.Confirmed) order.ConfirmedAt ??= DateTime.UtcNow;
        if (status == OrderStatus.Shipped) order.ShippedAt ??= DateTime.UtcNow;
        if (status == OrderStatus.Delivered) order.DeliveredAt ??= DateTime.UtcNow;

        _unitOfWork.Repository<Order>().Update(order);
        await _unitOfWork.CompleteAsync();

        return RedirectToAction(nameof(Details), new { id });
    }
}
