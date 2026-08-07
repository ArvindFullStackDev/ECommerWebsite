using AutoMapper;
using Application.Common.Exceptions;
using Catalog.DTOs;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using Shared.Models;

namespace Catalog.Queries;

public class GetBrandByIdQuery : IRequest<ApiResponse<BrandDto>>
{
    public int Id { get; set; }
}

public class GetBrandByIdQueryHandler : IRequestHandler<GetBrandByIdQuery, ApiResponse<BrandDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetBrandByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<BrandDto>> Handle(GetBrandByIdQuery request, CancellationToken cancellationToken)
    {
        var brand = await _unitOfWork.Repository<Brand>().GetByIdAsync(request.Id);
        if (brand == null)
            throw new NotFoundException(nameof(Brand), request.Id);

        var dto = _mapper.Map<BrandDto>(brand);
        dto.ProductCount = await _unitOfWork.Repository<Product>()
            .CountAsync(p => p.BrandId == brand.Id && p.IsActive);

        return ApiResponse<BrandDto>.Ok(dto);
    }
}
