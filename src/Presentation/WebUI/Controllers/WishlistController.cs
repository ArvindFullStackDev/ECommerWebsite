using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebUI.Controllers;

public class WishlistController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public WishlistController(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<IActionResult> Index()
    {
        if (!_currentUser.IsAuthenticated)
            return RedirectToAction("Login", "Account");

        var wishlistItems = await _unitOfWork.Repository<Wishlist>().GetQueryable()
            .Include(w => w.Product)
                .ThenInclude(p => p.Images)
            .Where(w => w.UserId == _currentUser.UserId)
            .ToListAsync();

        return View(wishlistItems);
    }

    [HttpPost]
    public async Task<JsonResult> AddToWishlist(int productId)
    {
        if (!_currentUser.IsAuthenticated)
            return Json(new { success = false, message = "Please login first" });

        var exists = await _unitOfWork.Repository<Wishlist>()
            .ExistsAsync(w => w.UserId == _currentUser.UserId && w.ProductId == productId);

        if (exists)
            return Json(new { success = false, message = "Already in wishlist" });

        var wishlistItem = new Wishlist
        {
            UserId = _currentUser.UserId!,
            ProductId = productId
        };

        await _unitOfWork.Repository<Wishlist>().AddAsync(wishlistItem);
        await _unitOfWork.CompleteAsync();

        var count = await _unitOfWork.Repository<Wishlist>()
            .CountAsync(w => w.UserId == _currentUser.UserId);

        return Json(new { success = true, wishlistCount = count, message = "Added to wishlist" });
    }

    [HttpPost]
    public async Task<IActionResult> RemoveFromWishlist(int id)
    {
        var item = await _unitOfWork.Repository<Wishlist>().GetByIdAsync(id);
        if (item != null)
        {
            _unitOfWork.Repository<Wishlist>().Delete(item);
            await _unitOfWork.CompleteAsync();
        }

        return RedirectToAction("Index");
    }
}
