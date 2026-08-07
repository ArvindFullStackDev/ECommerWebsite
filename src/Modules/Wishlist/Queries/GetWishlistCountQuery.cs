using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Wishlist.Queries;

public record GetWishlistCountQuery(string UserId) : IRequest<int>;

public class GetWishlistCountQueryHandler : IRequestHandler<GetWishlistCountQuery, int>
{
    private readonly IUnitOfWork _unitOfWork;
    public GetWishlistCountQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
    public async Task<int> Handle(GetWishlistCountQuery request, CancellationToken ct)
    {
        return await _unitOfWork.Repository<Wishlist>().GetQueryable()
            .CountAsync(w => w.UserId == request.UserId, ct);
    }
}