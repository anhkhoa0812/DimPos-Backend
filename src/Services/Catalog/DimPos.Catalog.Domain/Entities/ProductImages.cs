using DimPos.Catalog.Domain.Entities.Common;

namespace DimPos.Catalog.Domain.Entities;

public class ProductImages : EntityBase<Guid>
{
    public string ImageUrl { get; set; }
    public bool IsMainImage { get; set; } = false;
    public string? AltText { get; set; }
    
    public Guid ProductId { get; set; }
    public virtual Products Product { get; set; }
    
}