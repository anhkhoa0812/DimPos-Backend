using DimPos.Catalog.Domain.Entities;
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
        builder.Property(c => c.Name)
            .HasMaxLength(200);
        builder.Property(c => c.Description)
            .HasMaxLength(1000);
        builder.Property(c => c.Type)
            .HasMaxLength(50);
        builder.Property(c => c.DisplayOrder);
        builder.Property(c => c.PictureUrl)
            .HasMaxLength(1000);
        builder.HasOne(c => c.Parent)
            .WithMany(c => c.Childrens)
            .HasForeignKey(c => c.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

    }
}