using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Reviews.DTOs;

namespace Reviews.Queries;

public record GetUserReviewsQuery(string UserId) : IRequest<List<ReviewDto>>;

public class GetUserReviewsQueryHandler : IRequestHandler<GetUserReviewsQuery, List<ReviewDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    public GetUserReviewsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
    public async Task<List<ReviewDto>> Handle(GetUserReviewsQuery request, CancellationToken ct)
    {
        return await _unitOfWork.Repository<Review>().GetQueryable()
            .Where(r => r.UserId == request.UserId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReviewDto
            {
                Id = r.Id, ProductId = r.ProductId, UserId = r.UserId, UserName = r.UserId,
                Rating = r.Rating, Comment = r.Comment, LikeCount = r.ReviewLikes.Count, CreatedAt = r.CreatedAt
            }).ToListAsync(ct);
    }
}
