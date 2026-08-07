using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.ToTable("ProductVariants");
        builder.HasKey(pv => pv.Id);
        builder.Property(pv => pv.Name).IsRequired().HasMaxLength(100);
        builder.Property(pv => pv.Value).IsRequired().HasMaxLength(200);
        builder.Property(pv => pv.PriceAdjustment).HasColumnType("decimal(18,2)");
        builder.Property(pv => pv.SKU).HasMaxLength(100);
    }
}
