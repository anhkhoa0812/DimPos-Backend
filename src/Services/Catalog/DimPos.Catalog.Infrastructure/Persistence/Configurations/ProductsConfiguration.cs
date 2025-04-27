using DimPos.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DimPos.Catalog.Infrastructure.Persistence.Configurations;

public class ProductsConfiguration : IEntityTypeConfiguration<Products>
{
    public void Configure(EntityTypeBuilder<Products> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Code)
            .HasMaxLength(50);
        builder.Property(p => p.Name)
            .HasMaxLength(200);
        builder.Property(p => p.Description)
            .HasMaxLength(1000);
        builder.Property(p => p.Introduction)
            .HasMaxLength(1000);
        builder.Property(p => p.AlternativeCode)
            .HasMaxLength(100);
        builder.Property(p => p.PriceCOGS)
            .HasPrecision(18, 4);
        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}