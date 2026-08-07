using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Amount).HasColumnType("decimal(18,2)");
        builder.Property(p => p.TransactionId).HasMaxLength(200);
        builder.Property(p => p.GatewayResponse).HasColumnType("nvarchar(max)");
        builder.Property(p => p.PaymentIntentId).HasMaxLength(200);
        builder.Property(p => p.Currency).HasMaxLength(10);
    }
}
