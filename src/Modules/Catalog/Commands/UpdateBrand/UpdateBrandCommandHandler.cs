using AutoMapper;
using Application.Common.Exceptions;
using Catalog.DTOs;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using Shared.Extensions;
using Shared.Models;

namespace Catalog.Commands.UpdateBrand;

public class UpdateBrandCommandHandler : IRequestHandler<UpdateBrandCommand, ApiResponse<BrandDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateBrandCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<BrandDto>> Handle(UpdateBrandCommand request, CancellationToken cancellationToken)
    {
        var brand = await _unitOfWork.Repository<Brand>().GetByIdAsync(request.Id);
        if (brand == null)
            throw new NotFoundException(nameof(Brand), request.Id);

        brand.Name = request.Name;
        brand.Slug = request.Name.ToSlug();
        brand.Description = request.Description;
        brand.LogoUrl = request.LogoUrl;
        brand.IsActive = request.IsActive;
        brand.DisplayOrder = request.DisplayOrder;

        _unitOfWork.Repository<Brand>().Update(brand);
        await _unitOfWork.CompleteAsync();

        var dto = _mapper.Map<BrandDto>(brand);
        return ApiResponse<BrandDto>.Ok(dto, "Brand updated successfully.");
    }
}
