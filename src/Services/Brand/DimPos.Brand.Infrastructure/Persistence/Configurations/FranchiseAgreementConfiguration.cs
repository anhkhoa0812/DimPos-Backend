using DimPos.Brand.Domain.Entities;
using DimPos.Brand.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DimPos.Brand.Infrastructure.Persistence.Configurations;

public class FranchiseAgreementConfiguration: IEntityTypeConfiguration<FranchiseAgreement>
{
    public void Configure(EntityTypeBuilder<FranchiseAgreement> builder)
    {
        builder.HasKey(fa => fa.Id);
        builder.Property(fa => fa.InitialFeeCents)
            .HasPrecision(10, 2);
        builder.Property(fa => fa.RoyaltyPercentage)
            .HasPrecision(10, 2);
        builder.HasOne(fa => fa.Brand)
            .WithMany(b => b.FranchiseAgreements)
            .HasForeignKey(fa => fa.BrandId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Property(mg => mg.Status)
            .HasConversion(
                v => v.ToString(),
                v => (EFranchiseAgreementStatus)Enum.Parse(typeof(EFranchiseAgreementStatus), v)
            );
    }
}