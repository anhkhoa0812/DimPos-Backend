using DimPos.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DimPos.Identity.Infrastructure.Persistence.Configurations;

public class AccountsConfiguration : IEntityTypeConfiguration<Accounts>
{
    public void Configure(EntityTypeBuilder<Accounts> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Code)
            .HasMaxLength(50);
        builder.Property(a => a.Username)
            .HasMaxLength(50);
        // builder.Property(a => a.)
        
    }
}