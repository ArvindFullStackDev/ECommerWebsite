using Domain.Entities;
using Domain.Interfaces;
using MediatR;

namespace Cart.Commands.UpdateCartItem;

public record UpdateCartItemCommand(int CartItemId, int Quantity) : IRequest<bool>;

public class UpdateCartItemCommandHandler : IRequestHandler<UpdateCartItemCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    public UpdateCartItemCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
    public async Task<bool> Handle(UpdateCartItemCommand request, CancellationToken ct)
    {
        var item = await _unitOfWork.Repository<CartItem>().GetByIdAsync(request.CartItemId);
        if (item == null) return false;
        if (request.Quantity <= 0)
            _unitOfWork.Repository<CartItem>().Delete(item);
        else
            item.Quantity = request.Quantity;
        await _unitOfWork.CompleteAsync();
        return true;
    }
}
