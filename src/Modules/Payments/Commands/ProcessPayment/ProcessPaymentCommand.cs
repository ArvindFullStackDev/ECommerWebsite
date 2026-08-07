using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Payments.DTOs;
using Payments.Interfaces;

namespace Payments.Commands.ProcessPayment;

public record ProcessPaymentCommand(int OrderId, string PaymentMethod, decimal Amount) : IRequest<PaymentDto>;

public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, PaymentDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEnumerable<IPaymentService> _paymentServices;
    public ProcessPaymentCommandHandler(IUnitOfWork unitOfWork, IEnumerable<IPaymentService> paymentServices)
    {
        _unitOfWork = unitOfWork;
        _paymentServices = paymentServices;
    }
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

        await _unitOfWork.Repository<Payment>().AddAsync(payment, ct);
        await _unitOfWork.CompleteAsync(ct);

        return new PaymentDto
        {
            Id = payment.Id, OrderId = payment.OrderId, Amount = payment.Amount,
            PaymentMethod = payment.PaymentMethod.ToString(), Status = payment.Status.ToString(),
            Currency = payment.Currency, CreatedAt = payment.CreatedAt
        };
    }
}
