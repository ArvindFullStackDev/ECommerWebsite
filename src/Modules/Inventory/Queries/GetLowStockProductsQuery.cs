using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Inventory.DTOs;

namespace Inventory.Queries;

public record GetLowStockProductsQuery(int Threshold = 5) : IRequest<List<StockDto>>;

public class GetLowStockProductsQueryHandler : IRequestHandler<GetLowStockProductsQuery, List<StockDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    public GetLowStockProductsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
    public async Task<List<StockDto>> Handle(GetLowStockProductsQuery request, CancellationToken ct)
    {
        return await _unitOfWork.Repository<Inventory>().GetQueryable()
            .Include(i => i.Product)
            .Where(i => (i.QuantityInStock - i.ReservedQuantity) <= request.Threshold)
            .Select(i => new StockDto
            {
                ProductId = i.ProductId, ProductName = i.Product!.Name,
                QuantityInStock = i.QuantityInStock, ReservedQuantity = i.ReservedQuantity,
                LowStockThreshold = i.LowStockThreshold, LastRestockedAt = i.LastRestockedAt
            }).ToListAsync(ct);
    }
}
