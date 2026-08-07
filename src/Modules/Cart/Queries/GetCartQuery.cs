using Cart.DTOs;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cart.Queries;

public record GetCartQuery(string UserId) : IRequest<CartDto?>;

public class GetCartQueryHandler : IRequestHandler<GetCartQuery, CartDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    public GetCartQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
    public async Task<CartDto?> Handle(GetCartQuery request, CancellationToken ct)
    {
        var cart = await _unitOfWork.Repository<Domain.Entities.Cart>().GetQueryable()
            .Include(c => c.Items.Where(i => !i.IsSavedForLater)).ThenInclude(i => i.Product).ThenInclude(p => p.Images)
            .FirstOrDefaultAsync(c => c.UserId == request.UserId, ct);

        if (cart == null) return null;

        return new CartDto
        {
            Id = cart.Id,
            SubTotal = cart.Items.Where(i => !i.IsSavedForLater).Sum(i => i.UnitPrice * i.Quantity),
            TotalItems = cart.Items.Count(i => !i.IsSavedForLater),
            Items = cart.Items.Where(i => !i.IsSavedForLater).Select(i => new CartItemDto
            {
                Id = i.Id, ProductId = i.ProductId, ProductName = i.Product?.Name ?? "",
                ProductImage = i.Product?.Images?.FirstOrDefault()?.ImageUrl,
                UnitPrice = i.UnitPrice, DiscountedPrice = i.Product?.DiscountPrice,
                Quantity = i.Quantity, TotalPrice = i.UnitPrice * i.Quantity
            }).ToList()
        };
    }
}
