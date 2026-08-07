using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Payments.DTOs;

namespace Payments.Queries;

public record GetPaymentByOrderQuery(int OrderId) : IRequest<PaymentDto?>;

public class GetPaymentByOrderQueryHandler : IRequestHandler<GetPaymentByOrderQuery, PaymentDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    public GetPaymentByOrderQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
    public async Task<PaymentDto?> Handle(GetPaymentByOrderQuery request, CancellationToken ct)
    {
        var payment = await _unitOfWork.Repository<Payment>().GetQueryable()
            .FirstOrDefaultAsync(p => p.OrderId == request.OrderId, ct);
        if (payment == null) return null;
        return new PaymentDto
        {
            Id = payment.Id, OrderId = payment.OrderId, Amount = payment.Amount,
            PaymentMethod = payment.PaymentMethod.ToString(), Status = payment.Status.ToString(),
            Currency = payment.Currency, TransactionId = payment.TransactionId, CreatedAt = payment.CreatedAt
        };
    }
}
