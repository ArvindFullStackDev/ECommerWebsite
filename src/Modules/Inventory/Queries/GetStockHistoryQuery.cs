using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Inventory.DTOs;

namespace Inventory.Queries;

public record GetStockHistoryQuery(int ProductId) : IRequest<List<StockHistoryDto>>;

public class GetStockHistoryQueryHandler : IRequestHandler<GetStockHistoryQuery, List<StockHistoryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    public GetStockHistoryQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
    public async Task<List<StockHistoryDto>> Handle(GetStockHistoryQuery request, CancellationToken ct)
    {
        return await _unitOfWork.Repository<StockHistory>().GetQueryable()
            .Where(h => h.ProductId == request.ProductId)
            .OrderByDescending(h => h.CreatedAt)
            .Select(h => new StockHistoryDto
            {
                Id = h.Id, ProductId = h.ProductId, ChangeType = h.ChangeType,
                QuantityChanged = h.QuantityChanged, NewQuantity = h.NewQuantity,
                Notes = h.Notes, CreatedAt = h.CreatedAt
            }).ToListAsync(ct);
    }
}
