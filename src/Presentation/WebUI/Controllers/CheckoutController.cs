using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebUI.Controllers;

[Authorize]
public class CheckoutController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public CheckoutController(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var cart = await _unitOfWork.Repository<Cart>().GetQueryable()
            .Include(c => c.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Images)
            .FirstOrDefaultAsync(c => c.UserId == _currentUser.UserId);

        if (cart == null || !cart.Items.Any(i => !i.IsSavedForLater))
            return RedirectToAction("Index", "Cart");

        var addresses = await _unitOfWork.Repository<Address>().GetQueryable()
            .Where(a => a.UserId == _currentUser.UserId)
            .ToListAsync();

        var model = new CheckoutViewModel
        {
            CartItems = cart.Items.Where(i => !i.IsSavedForLater).ToList(),
            SubTotal = cart.Items.Where(i => !i.IsSavedForLater).Sum(i => i.UnitPrice * i.Quantity),
            Addresses = addresses,
            PaymentMethods = Enum.GetValues<PaymentMethod>().ToList()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PlaceOrder(PlaceOrderModel model)
    {
        if (!ModelState.IsValid) return RedirectToAction("Index");

        var cart = await _unitOfWork.Repository<Cart>().GetQueryable()
            .Include(c => c.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.UserId == _currentUser.UserId);

        if (cart == null || !cart.Items.Any())
            return RedirectToAction("Index", "Cart");

        var activeItems = cart.Items.Where(i => !i.IsSavedForLater).ToList();
        var subTotal = activeItems.Sum(i => i.UnitPrice * i.Quantity);
        var shippingCharge = subTotal >= 50 ? 0 : 5.99m;
        var taxRate = 0.085m;
        var taxAmount = subTotal * taxRate;

        var order = new Order
        {
            OrderNumber = GenerateOrderNumber(),
            UserId = _currentUser.UserId!,
            Status = OrderStatus.Pending,
            SubTotal = subTotal,
            TaxAmount = taxAmount,
            ShippingCharge = shippingCharge,
            DiscountAmount = 0,
            GrandTotal = subTotal + taxAmount + shippingCharge,
            PaymentMethod = model.PaymentMethod,
            PaymentStatus = PaymentStatus.Pending,
            ShippingMethod = model.ShippingMethod,
            ShippingAddressId = model.ShippingAddressId
        };

        foreach (var item in activeItems)
        {
            order.Items.Add(new OrderItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TotalPrice = item.UnitPrice * item.Quantity
            });
        }

        await _unitOfWork.Repository<Order>().AddAsync(order);

        foreach (var item in activeItems)
            _unitOfWork.Repository<CartItem>().Delete(item);

        var payment = new Payment
        {
            Amount = order.GrandTotal,
            PaymentMethod = model.PaymentMethod,
            Status = model.PaymentMethod == PaymentMethod.CashOnDelivery ? PaymentStatus.Pending : PaymentStatus.Processing,
            Currency = "USD",
            Order = order
        };
        await _unitOfWork.Repository<Payment>().AddAsync(payment);

        await _unitOfWork.CompleteAsync();

        return RedirectToAction("Confirmation", new { id = order.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Confirmation(int id)
    {
        var order = await _unitOfWork.Repository<Order>().GetQueryable()
            .Include(o => o.Items).ThenInclude(oi => oi.Product)
            .Include(o => o.ShippingAddress)
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == _currentUser.UserId);

        if (order == null) return RedirectToAction("Index", "Home");

        return View(order);
    }

    private static string GenerateOrderNumber()
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(10000, 99999)}";
    }
}

public class CheckoutViewModel
{
    public List<CartItem> CartItems { get; set; } = new();
    public decimal SubTotal { get; set; }
    public decimal ShippingCharge => SubTotal >= 50 ? 0 : 5.99m;
    public decimal TaxAmount => SubTotal * 0.085m;
    public decimal GrandTotal => SubTotal + TaxAmount + ShippingCharge;
    public List<Address> Addresses { get; set; } = new();
    public List<PaymentMethod> PaymentMethods { get; set; } = new();
}

public class PlaceOrderModel
{
    public int ShippingAddressId { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public ShippingMethod ShippingMethod { get; set; } = ShippingMethod.Standard;
}