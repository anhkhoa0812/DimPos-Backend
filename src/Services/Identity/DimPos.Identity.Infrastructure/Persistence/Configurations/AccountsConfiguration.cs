using DimPos.Identity.Domain.Entities;
using DimPos.Identity.Domain.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DimPos.Identity.Infrastructure.Persistence.Configurations;

public class AccountsConfiguration : IEntityTypeConfiguration<Accounts>
{
    public void Configure(EntityTypeBuilder<Accounts> builder)
    {
        builder.HasKey(a => a.Id);
        builder.HasIndex(a => a.Username)
            .IsUnique();
        builder.HasIndex(a => a.Code)
            .IsUnique();
        builder.Property(a => a.Username)
            .IsRequired()
            .HasMaxLength(50);
        builder.Property(a => a.Code)
            .IsRequired()
            .HasMaxLength(50);
        builder.Property(a => a.PasswordHash)
            .IsRequired();
        builder.Property(a => a.PasswordSalt)
            .IsRequired();
        builder.Property(a => a.RoleId)
            .IsRequired();
        builder.HasOne(a => a.Role)
            .WithMany(r => r.Accounts)
            .HasForeignKey(a => a.RoleId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Property(a => a.Status)
            .IsRequired()
            .HasConversion(
                v => v.ToString(),
                v => (EAccountStatus)Enum.Parse(typeof(EAccountStatus), v));

    }
}