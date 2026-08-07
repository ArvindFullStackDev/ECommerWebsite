using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Reporting.DTOs;

namespace Reporting.Queries;

public record GetTopProductsQuery(int Count = 10) : IRequest<List<TopProductDto>>;

public class GetTopProductsQueryHandler : IRequestHandler<GetTopProductsQuery, List<TopProductDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    public GetTopProductsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
    public async Task<List<TopProductDto>> Handle(GetTopProductsQuery request, CancellationToken ct)
    {
        return await _unitOfWork.Repository<OrderItem>().GetQueryable()
            .Include(oi => oi.Order)
            .Include(oi => oi.Product)
            .Where(oi => oi.Order!.Status == OrderStatus.Delivered)
            .GroupBy(oi => new { oi.ProductId, oi.Product!.Name })
            .Select(g => new TopProductDto
            {
                ProductId = g.Key.ProductId, ProductName = g.Key.Name,
                TotalSold = g.Sum(oi => oi.Quantity),
                TotalRevenue = g.Sum(oi => oi.TotalPrice)
            })
            .OrderByDescending(p => p.TotalSold)
            .Take(request.Count)
            .ToListAsync(ct);
    }
}
