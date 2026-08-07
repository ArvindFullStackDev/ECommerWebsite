using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class WalletTransactionConfiguration : IEntityTypeConfiguration<WalletTransaction>
{
    public void Configure(EntityTypeBuilder<WalletTransaction> builder)
    {
        builder.ToTable("WalletTransactions");
        builder.HasKey(wt => wt.Id);
        builder.Property(wt => wt.Amount).HasColumnType("decimal(18,2)");
        builder.Property(wt => wt.BalanceBefore).HasColumnType("decimal(18,2)");
        builder.Property(wt => wt.BalanceAfter).HasColumnType("decimal(18,2)");
        builder.Property(wt => wt.Type).IsRequired().HasMaxLength(50);
        builder.Property(wt => wt.Description).HasMaxLength(500);
    }
}
