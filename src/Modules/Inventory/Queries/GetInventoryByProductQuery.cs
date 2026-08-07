using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Inventory.DTOs;

namespace Inventory.Queries;

public record GetInventoryByProductQuery(int ProductId) : IRequest<StockDto?>;

public class GetInventoryByProductQueryHandler : IRequestHandler<GetInventoryByProductQuery, StockDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    public GetInventoryByProductQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
    public async Task<StockDto?> Handle(GetInventoryByProductQuery request, CancellationToken ct)
    {
        var inv = await _unitOfWork.Repository<Inventory>().GetQueryable()
            .Include(i => i.Product)
            .FirstOrDefaultAsync(i => i.ProductId == request.ProductId, ct);
        if (inv == null) return null;
        return new StockDto
        {
            ProductId = inv.ProductId, ProductName = inv.Product?.Name ?? "",
            QuantityInStock = inv.QuantityInStock, ReservedQuantity = inv.ReservedQuantity,
            LowStockThreshold = inv.LowStockThreshold, LastRestockedAt = inv.LastRestockedAt
        };
    }
}
