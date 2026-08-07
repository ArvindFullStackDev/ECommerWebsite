using Application.Common.Exceptions;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using Shared.Models;

namespace Catalog.Commands.DeleteCategory;

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, ApiResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCategoryCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _unitOfWork.Repository<Category>().GetByIdAsync(request.Id);
        if (category == null)
            throw new NotFoundException(nameof(Category), request.Id);

        // Check if category has subcategories
        var hasSubCategories = await _unitOfWork.Repository<Category>().ExistsAsync(c => c.ParentCategoryId == request.Id);
        if (hasSubCategories)
            return ApiResponse.Fail("Cannot delete category with subcategories. Remove subcategories first.");

        // Check if category has products
        var hasProducts = await _unitOfWork.Repository<Product>().ExistsAsync(p => p.CategoryId == request.Id);
        if (hasProducts)
            return ApiResponse.Fail("Cannot delete category with products. Move products to another category first.");

        _unitOfWork.Repository<Category>().SoftDelete(category);
        await _unitOfWork.CompleteAsync();

        return ApiResponse.Ok("Category deleted successfully.");
    }
}
