using AutoMapper;
using Application.Common.Exceptions;
using Catalog.DTOs;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Models;

namespace Catalog.Queries;

public class GetCategoryByIdQuery : IRequest<ApiResponse<CategoryDto>>
{
    public int Id { get; set; }
}

public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, ApiResponse<CategoryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetCategoryByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<CategoryDto>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var category = await _unitOfWork.Repository<Category>().GetQueryable()
            .Include(c => c.SubCategories)
            .Include(c => c.ParentCategory)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category == null)
            throw new NotFoundException(nameof(Category), request.Id);

        var dto = _mapper.Map<CategoryDto>(category);
        dto.ProductCount = await _unitOfWork.Repository<Product>()
            .CountAsync(p => p.CategoryId == category.Id && p.IsActive);

        return ApiResponse<CategoryDto>.Ok(dto);
    }
}
