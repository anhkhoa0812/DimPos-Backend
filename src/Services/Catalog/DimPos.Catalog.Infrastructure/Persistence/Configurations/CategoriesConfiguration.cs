using DimPos.Catalog.Domain.Entities;
using DimPos.Catalog.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DimPos.Catalog.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures the entity mapping for the <see cref="Categories"/> class with EF Core.
/// </summary>
/// <remarks>
/// This configuration class defines the table structure, primary keys, column properties,
/// and constraints for the <see cref="Categories"/> entity.
/// </remarks>
public class CategoriesConfiguration : IEntityTypeConfiguration<Categories>
{
    public void Configure(EntityTypeBuilder<Categories> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Code)
            .HasMaxLength(50);
        builder.HasIndex(c => c.Code)
            .IsUnique();
        builder.Property(c => c.BrandId)
            .IsRequired();
        builder.Property(c => c.HasChildCategory)
            .IsRequired();
        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);
        builder.Property(c => c.Description)
            .HasMaxLength(1000);
        builder.Property(c => c.Type)
            .IsRequired()
            .HasMaxLength(50);
        builder.Property(c => c.DisplayOrder);
        builder.Property(c => c.PictureUrl)
            .HasMaxLength(1000);
        builder.Property(c => c.Status)
            .HasConversion(
                v => v.ToString(),
                v => (ECategoryStatus)Enum.Parse(typeof(ECategoryStatus), v)
            );
        builder.HasOne(x => x.Parent)
            .WithMany(x => x.ChildCategories)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Property(p => p.Type)
            .HasConversion(
                v => v.ToString(),
                v => (ECategoryType)Enum.Parse(typeof(ECategoryType), v)
            );
    }
}