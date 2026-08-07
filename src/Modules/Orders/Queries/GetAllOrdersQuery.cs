using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Orders.DTOs;

namespace Orders.Queries;

public record GetAllOrdersQuery(string UserId) : IRequest<List<OrderListDto>>;

public class GetAllOrdersQueryHandler : IRequestHandler<GetAllOrdersQuery, List<OrderListDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    public GetAllOrdersQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
    public async Task<List<OrderListDto>> Handle(GetAllOrdersQuery request, CancellationToken ct)
    {
        return await _unitOfWork.Repository<Order>().GetQueryable()
            .Where(o => o.UserId == request.UserId)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new OrderListDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                Status = o.Status.ToString(),
                GrandTotal = o.GrandTotal,
                ItemCount = o.Items.Count,
                CreatedAt = o.CreatedAt
            }).ToListAsync(ct);
    }
}
