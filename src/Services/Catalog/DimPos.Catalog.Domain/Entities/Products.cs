using DimPos.Catalog.Domain.Entities.Common;

namespace DimPos.Catalog.Domain.Entities;

public class Products : EntityAuditBase<Guid>
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? IsHasVariants { get; set; } = false;
    public bool? IsHasRecipe { get; set; } = false;
    public int? Status { get; set; }
    public bool? IsAvailable { get; set; } = true;
    public int? DisplayOrder { get; set; }
    public bool? IsFixedPrice { get; set; } = false;
    public int? PosX { get; set; }
    public int? PosY { get; set; }
    public bool? IsMenuDisplay { get; set; } = false;
    public int? MaxExtra { get; set; }
    public string? Introduction { get; set; }
    public string? WebContent { get; set; }
    public bool? Active { get; set; } = true;
    public bool? IsDefaultChildProduct { get; set; } = false;
    public int? Positon { get; set; }
    public int? SaleType { get; set; }
    public bool? IsMostOrdered { get; set; } = false;
    public string? Note { get; set; }
    public int? NumOfUserVoted { get; set; }
    public string? AlternativeCode { get; set; }
    public decimal? PriceCOGS { get; set; }
    public Guid? BrandId { get; set; }
    public int? PackagingId { get; set; }
    
    public Guid? CategoryId { get; set; }
    
    public virtual Categories? Category { get; set; }
    public virtual ICollection<ProductAttributes>? ProductAttributes { get; set; } = new List<ProductAttributes>();
    public virtual ICollection<ProductImages>? ProductImages { get; set; } = new List<ProductImages>();
    public virtual ICollection<ProductVariants>? ProductVariants { get; set; } = new List<ProductVariants>();
}