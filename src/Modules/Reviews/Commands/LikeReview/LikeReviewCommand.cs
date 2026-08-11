using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Reviews.Commands.LikeReview;

public record LikeReviewCommand(int ReviewId, string UserId) : IRequest<bool>;

public class LikeReviewCommandHandler : IRequestHandler<LikeReviewCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    public LikeReviewCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
    public async Task<bool> Handle(LikeReviewCommand request, CancellationToken ct)
    {
        var existing = await _unitOfWork.Repository<ReviewLike>().GetQueryable()
            .FirstOrDefaultAsync(l => l.ReviewId == request.ReviewId && l.UserId == request.UserId, ct);
        if (existing != null) return false;

        await _unitOfWork.Repository<ReviewLike>().AddAsync(new ReviewLike
        {
            ReviewId = request.ReviewId, UserId = request.UserId
        });
        await _unitOfWork.CompleteAsync();
        return true;
    }
}
