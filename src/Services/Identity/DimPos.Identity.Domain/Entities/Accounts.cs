using DimPos.Identity.Domain.Entities.Common;
using DimPos.Identity.Domain.Enum;

namespace DimPos.Identity.Domain.Entities;

public class Accounts : EntityAuditBase<Guid>
{
    public string? Code { get; set; }
    public string? Username { get; set; }
    public byte[]? PasswordHash { get; set; }
    public byte[]? PasswordSalt { get; set; }
    public string? Email { get; set; }
    public EAccountStatus Status { get; set; }
    
    public Guid RoleId { get; set; }
    public virtual Role Role { get; set; }
}