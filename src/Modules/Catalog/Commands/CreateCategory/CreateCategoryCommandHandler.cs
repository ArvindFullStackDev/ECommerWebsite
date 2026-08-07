using AutoMapper;
using Catalog.DTOs;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using Shared.Extensions;
using Shared.Models;

namespace Catalog.Commands.CreateCategory;

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, ApiResponse<CategoryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateCategoryCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<CategoryDto>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = new Category
        {
            Name = request.Name,
            Slug = request.Name.ToSlug(),
            Description = request.Description,
            ImageUrl = request.ImageUrl,
            ParentCategoryId = request.ParentCategoryId,
            IsActive = request.IsActive,
            DisplayOrder = request.DisplayOrder
        };

        var result = await _unitOfWork.Repository<Category>().AddAsync(category);
        await _unitOfWork.CompleteAsync();

        var dto = _mapper.Map<CategoryDto>(result);
        return ApiResponse<CategoryDto>.Ok(dto, "Category created successfully.");
    }
}
