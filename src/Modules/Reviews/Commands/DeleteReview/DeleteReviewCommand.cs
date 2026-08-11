using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Reviews.Commands.DeleteReview;

public record DeleteReviewCommand(int ReviewId, string UserId) : IRequest<bool>;

public class DeleteReviewCommandHandler : IRequestHandler<DeleteReviewCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    public DeleteReviewCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
    public async Task<bool> Handle(DeleteReviewCommand request, CancellationToken ct)
    {
        var review = await _unitOfWork.Repository<Review>().GetQueryable()
            .FirstOrDefaultAsync(r => r.Id == request.ReviewId && r.UserId == request.UserId, ct);
        if (review == null) return false;
        _unitOfWork.Repository<Review>().Delete(review);
        await _unitOfWork.CompleteAsync();
        return true;
    }
}
