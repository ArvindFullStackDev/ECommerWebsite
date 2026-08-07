using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Commands.ReserveStock;

public record ReserveStockCommand(int ProductId, int Quantity) : IRequest<bool>;

public class ReserveStockCommandHandler : IRequestHandler<ReserveStockCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    public ReserveStockCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
    public async Task<bool> Handle(ReserveStockCommand request, CancellationToken ct)
    {
        var inventory = await _unitOfWork.Repository<Inventory>().GetQueryable()
            .FirstOrDefaultAsync(i => i.ProductId == request.ProductId, ct);
        if (inventory == null || inventory.QuantityInStock - inventory.ReservedQuantity < request.Quantity)
            return false;
        inventory.ReservedQuantity += request.Quantity;
        await _unitOfWork.CompleteAsync(ct);
        return true;
    }
}
