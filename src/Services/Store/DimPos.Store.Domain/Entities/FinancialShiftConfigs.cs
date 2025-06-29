using DimPos.Store.Domain.Entities.Common;

namespace DimPos.Store.Domain.Entities;

public class FinancialShiftConfigs : EntityAuditBase<Guid>
{
    public Guid StoreId { get; set; }
    public TimeOnly OpeningTime { get; set; }
    public TimeOnly ClosingTime { get; set; }
    public Guid CreatedByAccountId { get; set; }
    
    public virtual Store Store { get; set; }
    public virtual ICollection<FinancialShifts>? FinancialShifts { get; set; } = new List<FinancialShifts>();
}