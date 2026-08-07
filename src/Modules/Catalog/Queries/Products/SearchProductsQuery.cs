using AutoMapper;
using AutoMapper.QueryableExtensions;
using Catalog.DTOs;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Models;

namespace Catalog.Queries.Products;

public class SearchProductsQuery : IRequest<ApiResponse<PagedResult<ProductDto>>>
{
    public string? SearchTerm { get; set; }
    public int? CategoryId { get; set; }
    public int? BrandId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int? MinRating { get; set; }
    public string? SortBy { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
    public bool? InStock { get; set; }
}

public class SearchProductsQueryHandler : IRequestHandler<SearchProductsQuery, ApiResponse<PagedResult<ProductDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SearchProductsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<PagedResult<ProductDto>>> Handle(SearchProductsQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Repository<Domain.Entities.Product>().GetQueryable()
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Images)
            .Where(p => p.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(term) ||
                p.ShortDescription!.ToLower().Contains(term) ||
                p.Tags!.ToLower().Contains(term) ||
                p.Category.Name.ToLower().Contains(term) ||
                p.Brand!.Name.ToLower().Contains(term));
        }

        if (request.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == request.CategoryId.Value);

        if (request.BrandId.HasValue)
            query = query.Where(p => p.BrandId == request.BrandId.Value);

        if (request.MinPrice.HasValue)
            query = query.Where(p => (p.DiscountPrice ?? p.Price) >= request.MinPrice.Value);

        if (request.MaxPrice.HasValue)
            query = query.Where(p => (p.DiscountPrice ?? p.Price) <= request.MaxPrice.Value);

        if (request.MinRating.HasValue)
            query = query.Where(p => p.AverageRating >= request.MinRating.Value);

        if (request.InStock.HasValue && request.InStock.Value)
            query = query.Where(p => p.StockQuantity > 0);

        query = request.SortBy?.ToLower() switch
        {
            "price_asc" => query.OrderBy(p => p.DiscountPrice ?? p.Price),
            "price_desc" => query.OrderByDescending(p => p.DiscountPrice ?? p.Price),
            "newest" => query.OrderByDescending(p => p.CreatedAt),
            "bestseller" => query.OrderByDescending(p => p.SoldCount),
            "rating" => query.OrderByDescending(p => p.AverageRating),
            _ => query.OrderByDescending(p => p.SoldCount)
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
