using Domain.Entities;
using Domain.Interfaces;
using MediatR;

namespace Cart.Commands.RemoveFromCart;

public record RemoveFromCartCommand(int CartItemId) : IRequest<bool>;

public class RemoveFromCartCommandHandler : IRequestHandler<RemoveFromCartCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    public RemoveFromCartCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
    public async Task<bool> Handle(RemoveFromCartCommand request, CancellationToken ct)
    {
        var item = await _unitOfWork.Repository<CartItem>().GetByIdAsync(request.CartItemId);
        if (item == null) return false;
        _unitOfWork.Repository<CartItem>().Delete(item);
        await _unitOfWork.CompleteAsync();
        return true;
    }
}
