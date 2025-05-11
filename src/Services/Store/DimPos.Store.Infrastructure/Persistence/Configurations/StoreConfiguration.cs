using DimPos.Store.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DimPos.Store.Infrastructure.Persistence.Configurations;

public class StoreConfiguration : IEntityTypeConfiguration<Domain.Entities.Store>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Store> builder)
    {
        builder.HasKey(s => s.Id);
        builder.HasIndex(s => s.Code)
            .IsUnique();
        builder.Property(s => s.Code)
            .HasMaxLength(50);
        builder.Property(s => s.Name)
            .HasMaxLength(500);
        builder.Property(s => s.ShortName)
            .HasMaxLength(100);
        builder.Property(s => s.Phone)
            .HasMaxLength(20);
        builder.Property(s => s.Email)
            .HasMaxLength(100);
        builder.Property(s => s.Description)
            .HasMaxLength(1000);
        builder.Property(s => s.Address)
            .HasMaxLength(1000);
        builder.Property(s => s.Type)
            .HasConversion(
                v => v.ToString(),
                v => (EStoreType)Enum.Parse(typeof(EStoreType), v)
            );
        builder.Property(s => s.Status)
            .HasConversion(
                v => v.ToString(),
                v => (EStoreStatus)Enum.Parse(typeof(EStoreStatus), v)
            );
    }
}