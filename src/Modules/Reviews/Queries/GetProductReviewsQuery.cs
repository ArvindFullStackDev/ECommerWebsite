using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Reviews.DTOs;

namespace Reviews.Queries;

public record GetProductReviewsQuery(int ProductId) : IRequest<List<ReviewDto>>;

public class GetProductReviewsQueryHandler : IRequestHandler<GetProductReviewsQuery, List<ReviewDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    public GetProductReviewsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
    public async Task<List<ReviewDto>> Handle(GetProductReviewsQuery request, CancellationToken ct)
    {
        return await _unitOfWork.Repository<Review>().GetQueryable()
            .Where(r => r.ProductId == request.ProductId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReviewDto
            {
                Id = r.Id, ProductId = r.ProductId, UserId = r.UserId, UserName = r.UserId,
                Rating = r.Rating, Title = r.Title, Comment = r.Comment,
                LikeCount = r.Likes.Count, CreatedAt = r.CreatedAt
            }).ToListAsync(ct);
    }
}
