using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Orders.Commands.CancelOrder;

public record CancelOrderCommand(int OrderId, string UserId, string? Reason) : IRequest<bool>;

public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    public CancelOrderCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
    public async Task<bool> Handle(CancelOrderCommand request, CancellationToken ct)
    {
        var order = await _unitOfWork.Repository<Order>().GetQueryable()
            .FirstOrDefaultAsync(o => o.Id == request.OrderId && o.UserId == request.UserId, ct);
        if (order == null || order.Status == OrderStatus.Delivered || order.Status == OrderStatus.Shipped)
            return false;
        order.Status = OrderStatus.Cancelled;
        order.CancellationReason = request.Reason;
        await _unitOfWork.CompleteAsync();
        return true;
    }
}
