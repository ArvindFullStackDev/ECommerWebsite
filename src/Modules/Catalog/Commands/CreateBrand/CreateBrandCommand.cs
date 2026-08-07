using MediatR;
using Catalog.DTOs;
using Shared.Models;

namespace Catalog.Commands.CreateBrand;

public class CreateBrandCommand : IRequest<ApiResponse<BrandDto>>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }
}
