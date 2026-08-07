using MediatR;
using Catalog.DTOs;
using Shared.Models;

namespace Catalog.Commands.UpdateCategory;

public class UpdateCategoryCommand : IRequest<ApiResponse<CategoryDto>>
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int? ParentCategoryId { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
}
