using DimPos.Identity.Domain.Entities.Common;
using DimPos.Identity.Domain.Enum;

namespace DimPos.Identity.Domain.Entities;

public class Role : EntityBase<Guid>
{
    public ERoleName? Name { get; set; }
    public string? ShortName { get; set; }
    
    public virtual ICollection<Accounts> Accounts { get; set; } = new List<Accounts>();
}