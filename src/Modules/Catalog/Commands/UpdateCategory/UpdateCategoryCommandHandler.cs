using AutoMapper;
using Application.Common.Exceptions;
using Catalog.DTOs;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using Shared.Extensions;
using Shared.Models;

namespace Catalog.Commands.UpdateCategory;

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, ApiResponse<CategoryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateCategoryCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<CategoryDto>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _unitOfWork.Repository<Category>().GetByIdAsync(request.Id);
        if (category == null)
            throw new NotFoundException(nameof(Category), request.Id);

        category.Name = request.Name;
        category.Slug = request.Name.ToSlug();
        category.Description = request.Description;
        category.ImageUrl = request.ImageUrl;
        category.ParentCategoryId = request.ParentCategoryId;
        category.IsActive = request.IsActive;
        category.DisplayOrder = request.DisplayOrder;

        _unitOfWork.Repository<Category>().Update(category);
        await _unitOfWork.CompleteAsync();

        var dto = _mapper.Map<CategoryDto>(category);
        return ApiResponse<CategoryDto>.Ok(dto, "Category updated successfully.");
    }
}
