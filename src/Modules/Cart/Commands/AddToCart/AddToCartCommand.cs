using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cart.Commands.AddToCart;

public record AddToCartCommand(string UserId, int ProductId, int Quantity = 1) : IRequest<bool>;

public class AddToCartCommandHandler : IRequestHandler<AddToCartCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    public AddToCartCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
    public async Task<bool> Handle(AddToCartCommand request, CancellationToken ct)
    {
        var product = await _unitOfWork.Repository<Product>().GetByIdAsync(request.ProductId);
        if (product == null) return false;

        var cartRepo = _unitOfWork.Repository<Domain.Entities.Cart>();
        var cart = await cartRepo.GetQueryable()
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == request.UserId, ct);
        if (cart == null)
        {
            cart = new Domain.Entities.Cart { UserId = request.UserId };
            await cartRepo.AddAsync(cart);
        }

        var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId && !i.IsSavedForLater);
        if (existingItem != null)
            existingItem.Quantity += request.Quantity;
        else
            cart.Items.Add(new CartItem
            {
                ProductId = request.ProductId, Quantity = request.Quantity,
                UnitPrice = product.DiscountPrice ?? product.Price
            });

        await _unitOfWork.CompleteAsync();
        return true;
    }
}
