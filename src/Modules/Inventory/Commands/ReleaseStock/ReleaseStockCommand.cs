using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Commands.ReleaseStock;

public record ReleaseStockCommand(int ProductId, int Quantity) : IRequest<bool>;

public class ReleaseStockCommandHandler : IRequestHandler<ReleaseStockCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    public ReleaseStockCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
    public async Task<bool> Handle(ReleaseStockCommand request, CancellationToken ct)
    {
        var inventory = await _unitOfWork.Repository<Inventory>().GetQueryable()
            .FirstOrDefaultAsync(i => i.ProductId == request.ProductId, ct);
        if (inventory == null || inventory.ReservedQuantity < request.Quantity) return false;
        inventory.ReservedQuantity -= request.Quantity;
        await _unitOfWork.CompleteAsync(ct);
        return true;
    }
}
