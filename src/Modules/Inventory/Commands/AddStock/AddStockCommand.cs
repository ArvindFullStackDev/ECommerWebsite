using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Commands.AddStock;

public record AddStockCommand(int ProductId, int Quantity, string? Notes) : IRequest<bool>;

public class AddStockCommandHandler : IRequestHandler<AddStockCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    public AddStockCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
    public async Task<bool> Handle(AddStockCommand request, CancellationToken ct)
    {
        var inventory = await _unitOfWork.Repository<Inventory>().GetQueryable()
            .FirstOrDefaultAsync(i => i.ProductId == request.ProductId, ct);
        if (inventory == null)
        {
            inventory = new Inventory { ProductId = request.ProductId, QuantityInStock = request.Quantity };
            await _unitOfWork.Repository<Inventory>().AddAsync(inventory, ct);
        }
        else
        {
            inventory.QuantityInStock += request.Quantity;
            inventory.LastRestockedAt = DateTime.UtcNow;
        }

        var history = new StockHistory
        {
            ProductId = request.ProductId, ChangeType = "Added",
            QuantityChanged = request.Quantity, NewQuantity = inventory.QuantityInStock,
            Notes = request.Notes
        };
        await _unitOfWork.Repository<StockHistory>().AddAsync(history, ct);
        await _unitOfWork.CompleteAsync(ct);
        return true;
    }
}
