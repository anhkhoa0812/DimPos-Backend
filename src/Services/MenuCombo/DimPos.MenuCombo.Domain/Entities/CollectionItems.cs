using DimPos.MenuCombo.Domain.Entities.Common;

namespace DimPos.MenuCombo.Domain.Entities;

public class CollectionItems : EntityBase<Guid>
{
    public int? DisplayOrder { get; set; }
    public int? Quantity { get; set; }
    public bool? IsMandatory { get; set; }
    public Guid? ProductVariantId { get; set; }
    public Guid? CollectionId { get; set; }
    
    public virtual Collections Collection { get; set; }
}