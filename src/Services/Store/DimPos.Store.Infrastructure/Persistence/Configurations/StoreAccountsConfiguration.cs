using DimPos.Store.Domain.Entities;
using DimPos.Store.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DimPos.Store.Infrastructure.Persistence.Configurations;

public class StoreAccountsConfiguration : IEntityTypeConfiguration<StoreAccounts>
{
    public void Configure(EntityTypeBuilder<StoreAccounts> builder)
    {
        builder.HasKey(sa => sa.Id);
        builder.Property(sa => sa.StoreId)
            .IsRequired();
        builder.Property(sa => sa.AccountId)
            .IsRequired();
        builder.Property(sa => sa.AssignAt)
            .IsRequired();
        builder.HasOne(sa => sa.Store)
            .WithMany(s => s.StoreAccounts)
            .HasForeignKey(sa => sa.StoreId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Property(sa => sa.Role)
            .IsRequired()
            .HasConversion(
                v => v.ToString(),
                v => (EStoreRole)Enum.Parse(typeof(EStoreRole), v)
            );
    }
}