using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Orders.DTOs;

namespace Orders.Queries;

public record GetOrderByIdQuery(int Id, string UserId) : IRequest<OrderDetailDto?>;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDetailDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    public GetOrderByIdQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
    public async Task<OrderDetailDto?> Handle(GetOrderByIdQuery request, CancellationToken ct)
    {
        var order = await _unitOfWork.Repository<Order>().GetQueryable()
            .Include(o => o.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Images)
            .FirstOrDefaultAsync(o => o.Id == request.Id && o.UserId == request.UserId, ct);
        if (order == null) return null;
        return new OrderDetailDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            Status = order.Status.ToString(),
            SubTotal = order.SubTotal,
            TaxAmount = order.TaxAmount,
            ShippingCharge = order.ShippingCharge,
            DiscountAmount = order.DiscountAmount,
            GrandTotal = order.GrandTotal,
            CouponCode = order.CouponCode,
            Notes = order.Notes,
            PaymentMethod = order.PaymentMethod.ToString(),
            PaymentStatus = order.PaymentStatus.ToString(),
            ShippingMethod = order.ShippingMethod.ToString(),
            TrackingNumber = order.TrackingNumber,
            CreatedAt = order.CreatedAt,
            ConfirmedAt = order.ConfirmedAt,
            ShippedAt = order.ShippedAt,
            DeliveredAt = order.DeliveredAt,
            CancellationReason = order.CancellationReason,
            Items = order.Items.Select(i => new OrderItemDto
            {
                ProductId = i.ProductId,
                ProductName = i.Product?.Name ?? "",
                ProductImage = i.Product?.Images?.FirstOrDefault()?.ImageUrl,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                TotalPrice = i.TotalPrice
            }).ToList()
        };
    }
}
