using Payments.DTOs;

namespace Payments.Interfaces;

public interface IPaymentService
{
    Task<PaymentResult> ProcessPaymentAsync(ProcessPaymentDto dto);
    Task<PaymentResult> RefundPaymentAsync(int paymentId, decimal amount);
    Task<PaymentResult> VerifyPaymentAsync(string transactionId);
}

public class PaymentResult
{
    public bool Success { get; set; }
    public string? TransactionId { get; set; }
    public string? Message { get; set; }
    public string Status { get; set; } = "Failed";
}
