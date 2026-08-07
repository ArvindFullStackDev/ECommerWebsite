using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cart.Queries;

public record GetCartCountQuery(string UserId) : IRequest<int>;

public class GetCartCountQueryHandler : IRequestHandler<GetCartCountQuery, int>
{
    private readonly IUnitOfWork _unitOfWork;
    public GetCartCountQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
    public async Task<int> Handle(GetCartCountQuery request, CancellationToken ct)
    {
        var cart = await _unitOfWork.Repository<Domain.Entities.Cart>().GetQueryable()
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == request.UserId, ct);
        return cart?.Items.Count(i => !i.IsSavedForLater) ?? 0;
    }
}
