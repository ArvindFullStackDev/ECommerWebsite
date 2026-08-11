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
        var inventoryRepo = _unitOfWork.Repository<Domain.Entities.Inventory>();
        var inventory = await inventoryRepo.GetQueryable()
            .FirstOrDefaultAsync(i => i.ProductId == request.ProductId, ct);
        if (inventory == null)
        {
            inventory = new Domain.Entities.Inventory { ProductId = request.ProductId, Quantity = request.Quantity };
            await inventoryRepo.AddAsync(inventory);
        }
        else
        {
            inventory.Quantity += request.Quantity;
        }

        await _unitOfWork.Repository<StockHistory>().AddAsync(new StockHistory
        {
            InventoryId = inventory.Id, QuantityChange = request.Quantity,
            NewStock = inventory.Quantity, Notes = request.Notes
        });
        await _unitOfWork.CompleteAsync();
        return true;
    }
}
