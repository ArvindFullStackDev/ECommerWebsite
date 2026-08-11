using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Orders.Commands.UpdateOrderStatus;

public record UpdateOrderStatusCommand(int OrderId, OrderStatus NewStatus, string? TrackingNumber) : IRequest<bool>;

public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    public UpdateOrderStatusCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
    public async Task<bool> Handle(UpdateOrderStatusCommand request, CancellationToken ct)
    {
        var order = await _unitOfWork.Repository<Order>().GetQueryable()
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, ct);
        if (order == null) return false;
        order.Status = request.NewStatus;
        if (request.NewStatus == OrderStatus.Confirmed) order.ConfirmedAt = DateTime.UtcNow;
        if (request.NewStatus == OrderStatus.Shipped) { order.ShippedAt = DateTime.UtcNow; order.TrackingNumber = request.TrackingNumber; }
        if (request.NewStatus == OrderStatus.Delivered) order.DeliveredAt = DateTime.UtcNow;
        await _unitOfWork.CompleteAsync();
        return true;
    }
}
