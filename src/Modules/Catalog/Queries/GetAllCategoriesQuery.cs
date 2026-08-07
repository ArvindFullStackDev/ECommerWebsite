using AutoMapper;
using AutoMapper.QueryableExtensions;
using Catalog.DTOs;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Models;

namespace Catalog.Queries;

public class GetAllCategoriesQuery : IRequest<ApiResponse<List<CategoryDto>>>
{
    public bool? IsActive { get; set; }
}

public class GetAllCategoriesQueryHandler : IRequestHandler<GetAllCategoriesQuery, ApiResponse<List<CategoryDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllCategoriesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<List<CategoryDto>>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Repository<Domain.Entities.Category>().GetQueryable()
            .Include(c => c.SubCategories)
            .Include(c => c.ParentCategory)
            .AsQueryable();

        if (request.IsActive.HasValue)
            query = query.Where(c => c.IsActive == request.IsActive.Value);

        var categories = await query
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ProjectTo<CategoryDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        // Get product counts
        foreach (var cat in categories)
        {
            cat.ProductCount = await _unitOfWork.Repository<Domain.Entities.Product>()
                .CountAsync(p => p.CategoryId == cat.Id && p.IsActive);
        }

        return ApiResponse<List<CategoryDto>>.Ok(categories);
    }
}
