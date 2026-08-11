using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Wishlist.Commands.AddToWishlist;

public record AddToWishlistCommand(string UserId, int ProductId) : IRequest<bool>;

public class AddToWishlistCommandHandler : IRequestHandler<AddToWishlistCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    public AddToWishlistCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
    public async Task<bool> Handle(AddToWishlistCommand request, CancellationToken ct)
    {
        var exists = await _unitOfWork.Repository<Domain.Entities.Wishlist>().GetQueryable()
            .AnyAsync(w => w.UserId == request.UserId && w.ProductId == request.ProductId, ct);
        if (exists) return false;

        await _unitOfWork.Repository<Domain.Entities.Wishlist>().AddAsync(new Domain.Entities.Wishlist
        {
            UserId = request.UserId, ProductId = request.ProductId
        });
        await _unitOfWork.CompleteAsync();
        return true;
    }
}
