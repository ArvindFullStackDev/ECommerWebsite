using MediatR;
using Shared.Models;

namespace Catalog.Commands.DeleteBrand;

public class DeleteBrandCommand : IRequest<ApiResponse>
{
    public int Id { get; set; }
}
