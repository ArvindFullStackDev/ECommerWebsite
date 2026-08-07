using AutoMapper;
using AutoMapper.QueryableExtensions;
using Catalog.DTOs;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Models;

namespace Catalog.Queries;

public class GetAllBrandsQuery : IRequest<ApiResponse<List<BrandDto>>>
{
    public bool? IsActive { get; set; }
}

public class GetAllBrandsQueryHandler : IRequestHandler<GetAllBrandsQuery, ApiResponse<List<BrandDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllBrandsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<List<BrandDto>>> Handle(GetAllBrandsQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Repository<Domain.Entities.Brand>().GetQueryable()
            .Where(b => !b.IsDeleted);

        if (request.IsActive.HasValue)
            query = query.Where(b => b.IsActive == request.IsActive.Value);

        var brands = await query
            .OrderBy(b => b.DisplayOrder)
            .ThenBy(b => b.Name)
            .ProjectTo<BrandDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        foreach (var brand in brands)
        {
            brand.ProductCount = await _unitOfWork.Repository<Domain.Entities.Product>()
                .CountAsync(p => p.BrandId == brand.Id && p.IsActive);
        }

        return ApiResponse<List<BrandDto>>.Ok(brands);
    }
}
