using Application.Common.Exceptions;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using Shared.Models;

namespace Catalog.Commands.DeleteBrand;

public class DeleteBrandCommandHandler : IRequestHandler<DeleteBrandCommand, ApiResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteBrandCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse> Handle(DeleteBrandCommand request, CancellationToken cancellationToken)
    {
        var brand = await _unitOfWork.Repository<Brand>().GetByIdAsync(request.Id);
        if (brand == null)
            throw new NotFoundException(nameof(Brand), request.Id);

        _unitOfWork.Repository<Brand>().SoftDelete(brand);
        await _unitOfWork.CompleteAsync();

        return ApiResponse.Ok("Brand deleted successfully.");
    }
}
