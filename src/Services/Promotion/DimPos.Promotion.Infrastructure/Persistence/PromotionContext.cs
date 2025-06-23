using System.Reflection;
using DimPos.Promotion.Domain.Entities;
using DimPos.Promotion.Domain.Entities.Common.Interface;
using Microsoft.EntityFrameworkCore;

namespace DimPos.Promotion.Infrastructure.Persistence;

public class PromotionContext : DbContext
{
    public PromotionContext(DbContextOptions<PromotionContext> options) : base(options)
    {
    }
    
    public DbSet<Campaigns> Campaigns { get; set; } = null!;
    public DbSet<CampaignStores> CampaignStores { get; set; } = null!;
    public DbSet<CampaignRuleLinks> CampaignRuleLinks { get; set; } = null!;
    public DbSet<PromotionRules> PromotionRules { get; set; } = null!;
    public DbSet<RuleConditions> RuleConditions { get; set; } = null!;
    public DbSet<RuleActions> RuleActions { get; set; } = null!;
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new())
    {
        var modified = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Modified ||
                        e.State == EntityState.Added ||
                        e.State == EntityState.Deleted);

        foreach (var item in modified)
            switch (item.State)
            {
                case EntityState.Added:
                    if (item.Entity is IDateTracking addedEntity)
                    {
                        addedEntity.CreatedDate = DateTime.UtcNow;
                        item.State = EntityState.Added;
                    }

                    break;
                case EntityState.Modified:
                    if (item.Entity is IDateTracking modifiedEntity)
                    {
                        Entry(item.Entity).Property("Id").IsModified = false;
                        modifiedEntity.LastModifiedDate = DateTime.UtcNow;
                        item.State = EntityState.Modified;
                    }

                    break;
            }

        var result = await base.SaveChangesAsync(cancellationToken);
        return result;
    }
}