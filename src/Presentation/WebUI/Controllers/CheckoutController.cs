using System.ComponentModel.DataAnnotations;
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
        var model = await BuildCheckoutViewModelAsync();
        if (model == null) return RedirectToAction("Index", "Cart");
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PlaceOrder(PlaceOrderModel model)
    {
        if (model.PaymentMethod == 0 || !Enum.IsDefined(model.PaymentMethod))
            ModelState.AddModelError(nameof(PlaceOrderModel.PaymentMethod), "Please select a payment method");
        if (model.ShippingMethod == 0 || !Enum.IsDefined(model.ShippingMethod))
            ModelState.AddModelError(nameof(PlaceOrderModel.ShippingMethod), "Please select a shipping method");

        var addresses = await _unitOfWork.Repository<Address>().GetQueryable()
            .Where(a => a.UserId == _currentUser.UserId)
            .ToListAsync();

        int? shippingAddressId = null;
        if (model.ShippingAddressId > 0)
        {
            var existing = addresses.FirstOrDefault(a => a.Id == model.ShippingAddressId);
            if (existing == null)
                ModelState.AddModelError(nameof(PlaceOrderModel.ShippingAddressId), "Selected address is invalid");
            else
                shippingAddressId = existing.Id;
        }
        else
        {
            if (string.IsNullOrWhiteSpace(model.FullName))
                ModelState.AddModelError(nameof(PlaceOrderModel.FullName), "Full name is required");
            if (string.IsNullOrWhiteSpace(model.PhoneNumber))
                ModelState.AddModelError(nameof(PlaceOrderModel.PhoneNumber), "Phone number is required");
            if (string.IsNullOrWhiteSpace(model.AddressLine1))
                ModelState.AddModelError(nameof(PlaceOrderModel.AddressLine1), "Address line 1 is required");
            if (string.IsNullOrWhiteSpace(model.City))
                ModelState.AddModelError(nameof(PlaceOrderModel.City), "City is required");
            if (string.IsNullOrWhiteSpace(model.State))
                ModelState.AddModelError(nameof(PlaceOrderModel.State), "State is required");
            if (string.IsNullOrWhiteSpace(model.ZipCode))
                ModelState.AddModelError(nameof(PlaceOrderModel.ZipCode), "Zip code is required");
        }

        if (!ModelState.IsValid)
        {
            var viewModel = await BuildCheckoutViewModelAsync();
            if (viewModel == null) return RedirectToAction("Index", "Cart");
            viewModel.Form = model;
            return View("Index", viewModel);
        }

        var cart = await _unitOfWork.Repository<Domain.Entities.Cart>().GetQueryable()
            .Include(c => c.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.UserId == _currentUser.UserId);

        if (cart == null || !cart.Items.Any())
            return RedirectToAction("Index", "Cart");

        var activeItems = cart.Items.Where(i => !i.IsSavedForLater).ToList();
        var subTotal = activeItems.Sum(i => i.UnitPrice * i.Quantity);
        var shippingCharge = model.ShippingMethod switch
        {
            ShippingMethod.Express => 1104.15m,
            ShippingMethod.Overnight => 2124.15m,
            _ => subTotal >= 4250 ? 0 : 509.15m
        };
        var taxRate = 0.085m;
        var taxAmount = subTotal * taxRate;

        if (shippingAddressId == null)
        {
            var newAddress = new Address
            {
                UserId = _currentUser.UserId!,
                FullName = model.FullName!,
                PhoneNumber = model.PhoneNumber!,
                AddressLine1 = model.AddressLine1!,
                AddressLine2 = model.AddressLine2,
                City = model.City!,
                State = model.State!,
                ZipCode = model.ZipCode!,
                Country = string.IsNullOrWhiteSpace(model.Country) ? "India" : model.Country!,
                IsDefault = !addresses.Any()
            };
            _unitOfWork.Repository<Address>().AddAsync(newAddress);
            await _unitOfWork.CompleteAsync();
            shippingAddressId = newAddress.Id;
        }

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
            ShippingAddressId = shippingAddressId
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
            Currency = "INR",
            Order = order
        };
        await _unitOfWork.Repository<Payment>().AddAsync(payment);

        await _unitOfWork.CompleteAsync();

        return RedirectToAction("Confirmation", new { id = order.Id });
    }

    private async Task<CheckoutViewModel?> BuildCheckoutViewModelAsync()
    {
        var cart = await _unitOfWork.Repository<Domain.Entities.Cart>().GetQueryable()
            .Include(c => c.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Images)
            .FirstOrDefaultAsync(c => c.UserId == _currentUser.UserId);

        if (cart == null || !cart.Items.Any(i => !i.IsSavedForLater))
            return null;

        var addresses = await _unitOfWork.Repository<Address>().GetQueryable()
            .Where(a => a.UserId == _currentUser.UserId)
            .ToListAsync();

        var defaultAddress = addresses.FirstOrDefault(a => a.IsDefault) ?? addresses.FirstOrDefault();

        return new CheckoutViewModel
        {
            CartItems = cart.Items.Where(i => !i.IsSavedForLater).ToList(),
            SubTotal = cart.Items.Where(i => !i.IsSavedForLater).Sum(i => i.UnitPrice * i.Quantity),
            Addresses = addresses,
            PaymentMethods = Enum.GetValues<PaymentMethod>().ToList(),
            Form = new PlaceOrderModel { ShippingAddressId = defaultAddress?.Id ?? 0 }
        };
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
    public decimal ShippingCharge => SubTotal >= 4250 ? 0 : 509.15m;
    public decimal TaxAmount => SubTotal * 0.085m;
    public decimal GrandTotal => SubTotal + TaxAmount + ShippingCharge;
    public List<Address> Addresses { get; set; } = new();
    public List<PaymentMethod> PaymentMethods { get; set; } = new();
    public PlaceOrderModel Form { get; set; } = new();
}

public class PlaceOrderModel
{
    public int ShippingAddressId { get; set; }
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? Country { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public ShippingMethod ShippingMethod { get; set; }
}
