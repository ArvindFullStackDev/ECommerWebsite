using Payments.DTOs;
using Payments.Interfaces;

namespace Payments.Services;

public class StripePaymentService : IPaymentService
{
    public async Task<PaymentResult> ProcessPaymentAsync(ProcessPaymentDto dto)
    {
        await Task.Delay(100);
        return new PaymentResult
        {
            Success = true,
            TransactionId = $"STRIPE-{Guid.NewGuid():N}",
            Message = "Payment processed via Stripe",
            Status = "Completed"
        };
    }

    public async Task<PaymentResult> RefundPaymentAsync(int paymentId, decimal amount)
    {
        await Task.Delay(100);
        return new PaymentResult { Success = true, TransactionId = $"REF-{Guid.NewGuid():N}", Message = $"Refund of  processed", Status = "Completed" };
    }

    public async Task<PaymentResult> VerifyPaymentAsync(string transactionId)
    {
        await Task.Delay(100);
        return new PaymentResult { Success = true, Status = "Completed" };
    }
}
