using MediatR;
using Shared.Models;

namespace Catalog.Commands.DeleteCategory;

public class DeleteCategoryCommand : IRequest<ApiResponse>
{
    public int Id { get; set; }
}
