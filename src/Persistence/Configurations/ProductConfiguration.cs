using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(300);
        builder.Property(p => p.Slug).IsRequired().HasMaxLength(300);
        builder.Property(p => p.ShortDescription).HasMaxLength(500);
        builder.Property(p => p.Description).HasColumnType("nvarchar(max)");
        builder.Property(p => p.Price).HasColumnType("decimal(18,2)");
        builder.Property(p => p.DiscountPrice).HasColumnType("decimal(18,2)");
        builder.Property(p => p.DiscountValue).HasColumnType("decimal(18,2)");
        builder.Property(p => p.FlashSalePrice).HasColumnType("decimal(18,2)");
        builder.Property(p => p.SKU).HasMaxLength(100);
        builder.Property(p => p.Barcode).HasMaxLength(100);
        builder.Property(p => p.Tags).HasMaxLength(500);
        builder.Property(p => p.Specifications).HasColumnType("nvarchar(max)");
        builder.Property(p => p.Weight).HasColumnType("decimal(18,2)");
        builder.Property(p => p.Dimensions).HasMaxLength(100);
        builder.Property(p => p.MetaTitle).HasMaxLength(300);
        builder.Property(p => p.MetaDescription).HasMaxLength(500);
        builder.HasIndex(p => p.Slug).IsUnique();
        builder.HasIndex(p => p.SKU).IsUnique().HasFilter("[SKU] IS NOT NULL");
        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(p => p.Brand)
            .WithMany(b => b.Products)
            .HasForeignKey(p => p.BrandId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
