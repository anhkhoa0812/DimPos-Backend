using DimPos.Store.Domain.Entities.Common;
using DimPos.Store.Domain.Enums;

namespace DimPos.Store.Domain.Entities;

public class Devices  : EntityAuditBase<Guid>
{
    public Guid StoreId { get; set; }
    public string Name { get; set; } = string.Empty;
    public EDeviceType Type { get; set; }
    public string DeviceIdentifier { get; set; } = string.Empty;
    public EDeviceRegistrationStatus RegistrationStatus { get; set; }
    public string IpAddressLastSeen { get; set; } = string.Empty;
    
    public virtual Store Store { get; set; }
}