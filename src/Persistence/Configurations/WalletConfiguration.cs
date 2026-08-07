using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class WalletConfiguration : IEntityTypeConfiguration<Wallet>
{
    public void Configure(EntityTypeBuilder<Wallet> builder)
    {
        builder.ToTable("Wallets");
        builder.HasKey(w => w.Id);
        builder.Property(w => w.UserId).IsRequired().HasMaxLength(450);
        builder.Property(w => w.Balance).HasColumnType("decimal(18,2)");
        builder.HasIndex(w => w.UserId).IsUnique();
    }
}
