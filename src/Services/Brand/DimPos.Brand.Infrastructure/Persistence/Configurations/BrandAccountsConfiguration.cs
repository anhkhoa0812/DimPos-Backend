using DimPos.Brand.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DimPos.Brand.Infrastructure.Persistence.Configurations;

public class BrandAccountsConfiguration: IEntityTypeConfiguration<BrandAccounts>
{
    public void Configure(EntityTypeBuilder<BrandAccounts> builder)
    {
        builder.HasKey(ba => ba.Id);
        builder.HasOne(ba => ba.Brand)
            .WithMany(b => b.BrandAccounts)
            .HasForeignKey(ba => ba.BrandId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}