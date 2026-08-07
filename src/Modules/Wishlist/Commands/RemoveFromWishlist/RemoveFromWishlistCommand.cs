using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Wishlist.Commands.RemoveFromWishlist;

public record RemoveFromWishlistCommand(string UserId, int ProductId) : IRequest<bool>;

public class RemoveFromWishlistCommandHandler : IRequestHandler<RemoveFromWishlistCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    public RemoveFromWishlistCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
    public async Task<bool> Handle(RemoveFromWishlistCommand request, CancellationToken ct)
    {
        var item = await _unitOfWork.Repository<Wishlist>().GetQueryable()
            .FirstOrDefaultAsync(w => w.UserId == request.UserId && w.ProductId == request.ProductId, ct);
        if (item == null) return false;
        _unitOfWork.Repository<Wishlist>().Delete(item);
        await _unitOfWork.CompleteAsync();
        return true;
    }
}