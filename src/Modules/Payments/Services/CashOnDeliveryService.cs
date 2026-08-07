using Payments.Interfaces;

namespace Payments.Services;

public class CashOnDeliveryService : IPaymentService
{
    public Task<PaymentResult> ProcessPaymentAsync(ProcessPaymentDto dto)
    {
        return Task.FromResult(new PaymentResult
        {
            Success = true,
            TransactionId = $"COD-{Guid.NewGuid():N}",
            Message = "Pay with cash on delivery",
            Status = "Pending"
        });
    }

    public Task<PaymentResult> RefundPaymentAsync(int paymentId, decimal amount)
    {
        return Task.FromResult(new PaymentResult { Success = true, Message = "COD refund processed", Status = "Completed" });
    }

    public Task<PaymentResult> VerifyPaymentAsync(string transactionId)
    {
        return Task.FromResult(new PaymentResult { Success = true, Status = "Pending" });
    }
}
