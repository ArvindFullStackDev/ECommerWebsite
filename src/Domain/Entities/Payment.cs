using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class Payment : BaseEntity
{
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string? TransactionId { get; set; }
    public string? GatewayResponse { get; set; }
    public string? PaymentIntentId { get; set; }
    public string? Currency { get; set; } = "USD";

    public int? OrderId { get; set; }
    public Order? Order { get; set; }
}
