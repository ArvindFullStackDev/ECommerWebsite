using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebUI.Controllers;

public class CartController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public CartController(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<IActionResult> Index()
    {
        if (!_currentUser.IsAuthenticated)
            return RedirectToAction("Login", "Account");

        var cart = await _unitOfWork.Repository<Cart>().GetQueryable()
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p.Images)
            .FirstOrDefaultAsync(c => c.UserId == _currentUser.UserId);

        if (cart == null)
        {
            cart = new Cart { UserId = _currentUser.UserId! };
            await _unitOfWork.Repository<Cart>().AddAsync(cart);
            await _unitOfWork.CompleteAsync();
        }

        return View(cart);
    }

    [HttpPost]
    public async Task<JsonResult> AddToCart(int productId, int quantity = 1)
    {
        if (!_currentUser.IsAuthenticated)
            return Json(new { success = false, message = "Please login to add items to cart" });

        var cart = await _unitOfWork.Repository<Cart>().GetQueryable()
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == _currentUser.UserId);

        if (cart == null)
        {
            cart = new Cart { UserId = _currentUser.UserId! };
            await _unitOfWork.Repository<Cart>().AddAsync(cart);
        }

        var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
            _unitOfWork.Repository<CartItem>().Update(existingItem);
        }
        else
        {
            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(productId);
            if (product == null)
                return Json(new { success = false, message = "Product not found" });

            cart.Items.Add(new CartItem
            {
                ProductId = productId,
                Quantity = quantity,
                UnitPrice = product.DiscountPrice ?? product.Price
            });
        }

        await _unitOfWork.CompleteAsync();
        var cartCount = cart.Items.Sum(i => i.Quantity);

        return Json(new { success = true, cartCount, message = "Added to cart" });
    }

    [HttpPost]
    public async Task<JsonResult> UpdateQuantity(int cartItemId, int quantity)
    {
        var cartItem = await _unitOfWork.Repository<CartItem>().GetByIdAsync(cartItemId);
        if (cartItem == null)
            return Json(new { success = false, message = "Item not found" });

        if (quantity <= 0)
            _unitOfWork.Repository<CartItem>().Delete(cartItem);
        else
        {
            cartItem.Quantity = quantity;
            _unitOfWork.Repository<CartItem>().Update(cartItem);
        }

        await _unitOfWork.CompleteAsync();
        return Json(new { success = true, message = "Cart updated" });
    }

    [HttpPost]
    public async Task<JsonResult> RemoveFromCart(int cartItemId)
    {
        var cartItem = await _unitOfWork.Repository<CartItem>().GetByIdAsync(cartItemId);
        if (cartItem == null)
            return Json(new { success = false, message = "Item not found" });

        _unitOfWork.Repository<CartItem>().Delete(cartItem);
        await _unitOfWork.CompleteAsync();

        return Json(new { success = true, message = "Item removed from cart" });
    }

    [HttpPost]
    public async Task<JsonResult> SaveForLater(int cartItemId)
    {
        var cartItem = await _unitOfWork.Repository<CartItem>().GetByIdAsync(cartItemId);
        if (cartItem == null)
            return Json(new { success = false });

        cartItem.IsSavedForLater = true;
        _unitOfWork.Repository<CartItem>().Update(cartItem);
        await _unitOfWork.CompleteAsync();

        return Json(new { success = true });
    }
}
