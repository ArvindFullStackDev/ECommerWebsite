using MediatR;
using Catalog.DTOs;
using Shared.Models;

namespace Catalog.Commands.UpdateBrand;

public class UpdateBrandCommand : IRequest<ApiResponse<BrandDto>>
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
}
