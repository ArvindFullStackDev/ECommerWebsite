using AutoMapper;
using AutoMapper.QueryableExtensions;
using Catalog.DTOs;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Models;

namespace Catalog.Queries.Products;

public class GetAllProductsQuery : IRequest<ApiResponse<PagedResult<ProductDto>>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
    public int? CategoryId { get; set; }
    public List<int>? CategoryIds { get; set; }
    public int? BrandId { get; set; }
    public bool? IsFeatured { get; set; }
    public bool? IsTrending { get; set; }
    public bool? IsBestSeller { get; set; }
    public string? SortBy { get; set; }
}

public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, ApiResponse<PagedResult<ProductDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllProductsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<PagedResult<ProductDto>>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Repository<Domain.Entities.Product>().GetQueryable()
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Images)
            .Where(p => p.IsActive)
            .AsQueryable();

        if (request.CategoryIds != null && request.CategoryIds.Count > 0)
            query = query.Where(p => request.CategoryIds.Contains(p.CategoryId));
        else if (request.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == request.CategoryId.Value);

        if (request.BrandId.HasValue)
            query = query.Where(p => p.BrandId == request.BrandId.Value);

        if (request.IsFeatured.HasValue)
            query = query.Where(p => p.IsFeatured == request.IsFeatured.Value);

        if (request.IsTrending.HasValue)
            query = query.Where(p => p.IsTrending == request.IsTrending.Value);

        if (request.IsBestSeller.HasValue)
            query = query.Where(p => p.IsBestSeller == request.IsBestSeller.Value);

        query = request.SortBy?.ToLower() switch
        {
            "price_asc" => query.OrderBy(p => p.DiscountPrice ?? p.Price),
            "price_desc" => query.OrderByDescending(p => p.DiscountPrice ?? p.Price),
            "newest" => query.OrderByDescending(p => p.CreatedAt),
            "bestseller" => query.OrderByDescending(p => p.SoldCount),
            "rating" => query.OrderByDescending(p => p.AverageRating),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectTo<ProductDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        var result = new PagedResult<ProductDto>
        {
            Items = items,
            PageIndex = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };

        return ApiResponse<PagedResult<ProductDto>>.Ok(result);
    }
}
