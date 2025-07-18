using DimPos.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DimPos.Catalog.Infrastructure.Persistence.Configurations;

/// <summary>
/// Provides the configuration for the <see cref="RecipeItems"/> entity used in the database context.
/// </summary>
/// <remarks>
/// This class configures the entity's schema settings, relationships, and constraints.
/// </remarks>
/// <example>
/// The configuration includes defining primary keys, required properties,
/// string length restrictions, and relationships with the <see cref="Ingredients"/>
/// and <see cref="Recipes"/> entities.
/// </example>

public class RecipeItemsConfiguration : IEntityTypeConfiguration<RecipeItems>
{
    public void Configure(EntityTypeBuilder<RecipeItems> builder)
    {
        builder.HasKey(ri => ri.Id);
        builder.HasIndex(ri => new { ri.ProductVariantId, ri.IngredientId })
            .IsUnique();
        builder.Property(ri => ri.ProductVariantId)
            .IsRequired();
        builder.Property(ri => ri.IngredientId)
            .IsRequired();
        builder.Property(ri => ri.Quantity)
            .IsRequired();
        builder.Property(ri => ri.UnitOfMeasureSnapshot)
            .IsRequired()
            .HasMaxLength(50);
        builder.HasOne(ri => ri.Ingredient)
            .WithMany(i => i.RecipeItems)
            .HasForeignKey(ri => ri.IngredientId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(ri => ri.ProductVariant)
            .WithMany(pv => pv.RecipeItems)
            .HasForeignKey(ri => ri.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}