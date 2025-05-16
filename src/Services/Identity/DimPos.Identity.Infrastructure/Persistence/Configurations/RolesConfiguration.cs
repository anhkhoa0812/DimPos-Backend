using DimPos.Identity.Domain.Entities;
using DimPos.Identity.Domain.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DimPos.Identity.Infrastructure.Persistence.Configurations;

public class RolesConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.ShortName)
            .IsRequired()
            .HasMaxLength(50);
        builder.Property(r => r.ShortName)
            .IsRequired();
        builder.Property(r => r.Name)
            .IsRequired()
            .HasConversion(
                v => v.ToString(),
                v => (ERoleName)Enum.Parse(typeof(ERoleName), v)
            );
    }
}