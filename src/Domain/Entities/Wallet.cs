using Domain.Common;

namespace Domain.Entities;

public class Wallet : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public decimal Balance { get; set; }

    public ICollection<WalletTransaction> Transactions { get; set; } = new List<WalletTransaction>();
}
