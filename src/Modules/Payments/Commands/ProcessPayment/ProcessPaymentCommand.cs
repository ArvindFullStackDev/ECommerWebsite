using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Payments.DTOs;

namespace Payments.Commands.ProcessPayment;

public record ProcessPaymentCommand(int OrderId, string PaymentMethod, decimal Amount) : IRequest<PaymentDto>;

public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, PaymentDto>
{
    private readonly IUnitOfWork _unitOfWork;
    public ProcessPaymentCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
    public async Task<PaymentDto> Handle(ProcessPaymentCommand request, CancellationToken ct)
    {
        var order = await _unitOfWork.Repository<Order>().GetQueryable()
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, ct);
        if (order == null) throw new InvalidOperationException("Order not found");

        var payment = new Payment
        {
            OrderId = order.Id,
            Amount = request.Amount,
            PaymentMethod = Enum.Parse<PaymentMethod>(request.PaymentMethod),
            Status = PaymentStatus.Processing,
            Currency = "USD"
        };

        await _unitOfWork.Repository<Payment>().AddAsync(payment);
        await _unitOfWork.CompleteAsync();

        return new PaymentDto
        {
            Id = payment.Id, OrderId = payment.OrderId ?? 0, Amount = payment.Amount,
            PaymentMethod = payment.PaymentMethod.ToString(), Status = payment.Status.ToString(),
            Currency = payment.Currency ?? "USD", CreatedAt = payment.CreatedAt
        };
    }
}
