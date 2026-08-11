using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Orders.Commands.CreateOrder;

public record CreateOrderCommand(
    string UserId, int ShippingAddressId, int? BillingAddressId,
    PaymentMethod PaymentMethod, ShippingMethod ShippingMethod,
    string? CouponCode, string? Notes) : IRequest<Order>;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Order>
{
    private readonly IUnitOfWork _unitOfWork;
    public CreateOrderCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
    public async Task<Order> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        var cartRepo = _unitOfWork.Repository<Domain.Entities.Cart>();
        var cart = await cartRepo.GetQueryable()
            .Include(c => c.Items.Where(i => !i.IsSavedForLater)).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.UserId == request.UserId, ct);
        if (cart == null || !cart.Items.Any()) throw new InvalidOperationException("Cart is empty");

        var activeItems = cart.Items.Where(i => !i.IsSavedForLater).ToList();
        var subTotal = activeItems.Sum(i => i.UnitPrice * i.Quantity);
        var shippingCharge = request.ShippingMethod == ShippingMethod.Standard && subTotal >= 4250 ? 0 : request.ShippingMethod switch
        {
            ShippingMethod.Express => 1104.15m,
            ShippingMethod.Overnight => 2124.15m,
            _ => 509.15m
        };
        var taxAmount = subTotal * 0.085m;
        var discountAmount = 0m;
        if (!string.IsNullOrEmpty(request.CouponCode))
        {
            var coupon = await _unitOfWork.Repository<Coupon>().GetQueryable()
                .FirstOrDefaultAsync(c => c.Code == request.CouponCode && c.IsActive && c.ValidTo >= DateTime.UtcNow, ct);
            if (coupon != null)
            {
                discountAmount = coupon.DiscountType == DiscountType.Percentage
                    ? subTotal * coupon.DiscountValue / 100m
                    : coupon.DiscountValue;
                if (discountAmount > subTotal) discountAmount = subTotal;
                coupon.UsedCount++;
            }
        }

        var order = new Order
        {
            OrderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(10000, 99999)}",
            UserId = request.UserId,
            Status = OrderStatus.Pending,
            SubTotal = subTotal, TaxAmount = taxAmount, ShippingCharge = shippingCharge,
            DiscountAmount = discountAmount, GrandTotal = subTotal + taxAmount + shippingCharge - discountAmount,
            PaymentMethod = request.PaymentMethod, PaymentStatus = PaymentStatus.Pending,
            ShippingMethod = request.ShippingMethod, ShippingAddressId = request.ShippingAddressId,
            BillingAddressId = request.BillingAddressId, CouponCode = request.CouponCode, Notes = request.Notes
        };

        foreach (var item in activeItems)
        {
            order.Items.Add(new OrderItem
            {
                ProductId = item.ProductId, Quantity = item.Quantity,
                UnitPrice = item.UnitPrice, TotalPrice = item.UnitPrice * item.Quantity
            });
        }

        await _unitOfWork.Repository<Order>().AddAsync(order);
        foreach (var item in activeItems) _unitOfWork.Repository<CartItem>().Delete(item);
        await _unitOfWork.CompleteAsync();
        return order;
    }
}
