using DimPos.Identity.Domain.Entities.Common;

namespace DimPos.Identity.Domain.Entities;

public class Role : EntityBase<Guid>
{
    public string? Name { get; set; }
    public string? ShortName { get; set; }
    
    public virtual ICollection<Accounts> Accounts { get; set; } = new List<Accounts>();
}