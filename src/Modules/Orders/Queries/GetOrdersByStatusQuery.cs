using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Orders.DTOs;

namespace Orders.Queries;

public record GetOrdersByStatusQuery(string UserId, OrderStatus Status) : IRequest<List<OrderListDto>>;

public class GetOrdersByStatusQueryHandler : IRequestHandler<GetOrdersByStatusQuery, List<OrderListDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    public GetOrdersByStatusQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
    public async Task<List<OrderListDto>> Handle(GetOrdersByStatusQuery request, CancellationToken ct)
    {
        return await _unitOfWork.Repository<Order>().GetQueryable()
            .Where(o => o.UserId == request.UserId && o.Status == request.Status)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new OrderListDto
            {
                Id = o.Id, OrderNumber = o.OrderNumber, Status = o.Status.ToString(),
                GrandTotal = o.GrandTotal, ItemCount = o.Items.Count, CreatedAt = o.CreatedAt
            }).ToListAsync(ct);
    }
}
