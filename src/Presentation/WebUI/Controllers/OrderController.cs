using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebUI.Controllers;

[Authorize]
public class OrderController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public OrderController(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
    {
        var query = _unitOfWork.Repository<Order>().GetQueryable()
            .Where(o => o.UserId == _currentUser.UserId)
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt);

        var total = await query.CountAsync();
        var orders = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        var model = new OrderListViewModel
        {
            Orders = orders,
            CurrentPage = page,
            TotalPages = (int)Math.Ceiling(total / (double)pageSize)
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var order = await _unitOfWork.Repository<Order>().GetQueryable()
            .Include(o => o.Items).ThenInclude(oi => oi.Product).ThenInclude(p => p.Images)
            .Include(o => o.ShippingAddress)
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == _currentUser.UserId);

        if (order == null) return RedirectToAction("Index");

        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var order = await _unitOfWork.Repository<Order>().GetQueryable()
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == _currentUser.UserId);

        if (order == null || order.Status == OrderStatus.Delivered || order.Status == OrderStatus.Shipped)
            return RedirectToAction("Details", new { id });

        order.Status = OrderStatus.Cancelled;
        await _unitOfWork.CompleteAsync();

        TempData["Success"] = "Order cancelled successfully.";
        return RedirectToAction("Details", new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Return(int id)
    {
        var order = await _unitOfWork.Repository<Order>().GetQueryable()
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == _currentUser.UserId);

        if (order == null || order.Status != OrderStatus.Delivered)
            return RedirectToAction("Details", new { id });

        order.Status = OrderStatus.Returned;
        await _unitOfWork.CompleteAsync();

        TempData["Success"] = "Return requested successfully.";
        return RedirectToAction("Details", new { id });
    }
}

public class OrderListViewModel
{
    public List<Order> Orders { get; set; } = new();
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
}