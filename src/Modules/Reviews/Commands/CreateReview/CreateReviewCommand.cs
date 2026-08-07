using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Reviews.DTOs;

namespace Reviews.Commands.CreateReview;

public record CreateReviewCommand(string UserId, int ProductId, int Rating, string? Title, string? Comment) : IRequest<ReviewDto>;

public class CreateReviewCommandHandler : IRequestHandler<CreateReviewCommand, ReviewDto>
{
    private readonly IUnitOfWork _unitOfWork;
    public CreateReviewCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
    public async Task<ReviewDto> Handle(CreateReviewCommand request, CancellationToken ct)
    {
        var existing = await _unitOfWork.Repository<Review>().GetQueryable()
            .FirstOrDefaultAsync(r => r.UserId == request.UserId && r.ProductId == request.ProductId, ct);
        if (existing != null) throw new InvalidOperationException("You already reviewed this product");

        var review = new Review
        {
            UserId = request.UserId, ProductId = request.ProductId,
            Rating = request.Rating, Title = request.Title, Comment = request.Comment
        };
        await _unitOfWork.Repository<Review>().AddAsync(review, ct);
        await _unitOfWork.CompleteAsync(ct);

        return new ReviewDto
        {
            Id = review.Id, ProductId = review.ProductId, UserId = review.UserId,
            Rating = review.Rating, Title = review.Title, Comment = review.Comment,
            LikeCount = 0, CreatedAt = review.CreatedAt
        };
    }
}
