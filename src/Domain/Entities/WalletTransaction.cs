using Domain.Common;

namespace Domain.Entities;

public class WalletTransaction : BaseEntity
{
    public decimal Amount { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal BalanceBefore { get; set; }
    public decimal BalanceAfter { get; set; }

    public int WalletId { get; set; }
    public Wallet Wallet { get; set; } = null!;
}
