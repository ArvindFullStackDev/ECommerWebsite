using AutoMapper;
using Catalog.DTOs;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using Shared.Extensions;
using Shared.Models;

namespace Catalog.Commands.CreateBrand;

public class CreateBrandCommandHandler : IRequestHandler<CreateBrandCommand, ApiResponse<BrandDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateBrandCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<BrandDto>> Handle(CreateBrandCommand request, CancellationToken cancellationToken)
    {
        var brand = new Brand
        {
            Name = request.Name,
            Slug = request.Name.ToSlug(),
            Description = request.Description,
            LogoUrl = request.LogoUrl,
            IsActive = request.IsActive,
            DisplayOrder = request.DisplayOrder
        };

        var result = await _unitOfWork.Repository<Brand>().AddAsync(brand);
        await _unitOfWork.CompleteAsync();

        var dto = _mapper.Map<BrandDto>(result);
        return ApiResponse<BrandDto>.Ok(dto, "Brand created successfully.");
    }
}
