using DimPos.Brand.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DimPos.Brand.Infrastructure.Persistence.Configurations;

public class BrandAccountsConfiguration: IEntityTypeConfiguration<BrandAccounts>
{
    public void Configure(EntityTypeBuilder<BrandAccounts> builder)
    {
        builder.HasKey(ba => ba.Id);
        builder.Property(ba => ba.AccountId)
            .IsRequired();
        builder.Property(ba => ba.BrandId)
            .IsRequired();
        builder.HasOne(ba => ba.Brand)
            .WithMany(b => b.BrandAccounts)
            .HasForeignKey(ba => ba.BrandId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}