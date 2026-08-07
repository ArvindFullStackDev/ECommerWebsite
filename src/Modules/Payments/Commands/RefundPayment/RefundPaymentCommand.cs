using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Payments.Commands.RefundPayment;

public record RefundPaymentCommand(int PaymentId, decimal Amount) : IRequest<bool>;

public class RefundPaymentCommandHandler : IRequestHandler<RefundPaymentCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    public RefundPaymentCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
    public async Task<bool> Handle(RefundPaymentCommand request, CancellationToken ct)
    {
        var payment = await _unitOfWork.Repository<Payment>().GetQueryable()
            .FirstOrDefaultAsync(p => p.Id == request.PaymentId, ct);
        if (payment == null || payment.Status == PaymentStatus.Refunded) return false;
        payment.Status = PaymentStatus.Refunded;
        await _unitOfWork.CompleteAsync(ct);
        return true;
    }
}
