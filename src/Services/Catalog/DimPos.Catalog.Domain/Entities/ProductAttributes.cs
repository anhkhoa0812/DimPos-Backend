using DimPos.Catalog.Domain.Entities.Common;

namespace DimPos.Catalog.Domain.Entities;

public class ProductAttributes : EntityBase<Guid>
{
    public string? Key { get; set; }
    public string? Value { get; set; }
    public Guid? ProductId { get; set; }
    public virtual Products? Product { get; set; }
    
}