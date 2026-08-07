using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wishlist.DTOs;

namespace Wishlist.Queries;

public record GetWishlistQuery(string UserId) : IRequest<WishlistDto>;

public class GetWishlistQueryHandler : IRequestHandler<GetWishlistQuery, WishlistDto>
{
    private readonly IUnitOfWork _unitOfWork;
    public GetWishlistQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
    public async Task<WishlistDto> Handle(GetWishlistQuery request, CancellationToken ct)
    {
        var items = await _unitOfWork.Repository<Wishlist>().GetQueryable()
            .Where(w => w.UserId == request.UserId)
            .Include(w => w.Product).ThenInclude(p => p.Images)
            .OrderByDescending(w => w.CreatedAt)
            .Select(w => new WishlistItemDto
            {
                Id = w.Id, ProductId = w.ProductId,
                ProductName = w.Product!.Name, ProductImage = w.Product.Images!.FirstOrDefault()!.ImageUrl,
                Price = w.Product.Price, DiscountedPrice = w.Product.DiscountPrice, AddedAt = w.CreatedAt
            }).ToListAsync(ct);

        return new WishlistDto { Items = items };
    }
}