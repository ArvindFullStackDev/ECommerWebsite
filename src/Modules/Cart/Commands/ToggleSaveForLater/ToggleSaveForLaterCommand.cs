using Domain.Entities;
using Domain.Interfaces;
using MediatR;

namespace Cart.Commands.ToggleSaveForLater;

public record ToggleSaveForLaterCommand(int CartItemId) : IRequest<bool>;

public class ToggleSaveForLaterCommandHandler : IRequestHandler<ToggleSaveForLaterCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    public ToggleSaveForLaterCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
    public async Task<bool> Handle(ToggleSaveForLaterCommand request, CancellationToken ct)
    {
        var item = await _unitOfWork.Repository<CartItem>().GetByIdAsync(request.CartItemId);
        if (item == null) return false;
        item.IsSavedForLater = !item.IsSavedForLater;
        await _unitOfWork.CompleteAsync();
        return true;
    }
}
