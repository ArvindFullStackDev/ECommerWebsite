using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Reporting.DTOs;

namespace Reporting.Queries;

public record GetSalesReportQuery(DateTime From, DateTime To) : IRequest<SalesReportDto>;

public class GetSalesReportQueryHandler : IRequestHandler<GetSalesReportQuery, SalesReportDto>
{
    private readonly IUnitOfWork _unitOfWork;
    public GetSalesReportQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
    public async Task<SalesReportDto> Handle(GetSalesReportQuery request, CancellationToken ct)
    {
        var orders = await _unitOfWork.Repository<Order>().GetQueryable()
            .Where(o => o.Status == OrderStatus.Delivered && o.CreatedAt >= request.From && o.CreatedAt <= request.To)
            .Include(o => o.Items)
            .ToListAsync(ct);

        var totalSales = orders.Sum(o => o.GrandTotal);
        var totalOrders = orders.Count;
        var totalProductsSold = orders.Sum(o => o.Items.Sum(i => i.Quantity));

        var dailySales = orders.GroupBy(o => o.CreatedAt.Date)
            .Select(g => new DailySalesDto
            {
                Date = g.Key, SalesAmount = g.Sum(o => o.GrandTotal),
                OrderCount = g.Count()
            }).OrderBy(d => d.Date).ToList();

        return new SalesReportDto
        {
            TotalSales = totalSales, TotalOrders = totalOrders,
            AverageOrderValue = totalOrders > 0 ? totalSales / totalOrders : 0,
            TotalProductsSold = totalProductsSold, DailySales = dailySales
        };
    }
}
