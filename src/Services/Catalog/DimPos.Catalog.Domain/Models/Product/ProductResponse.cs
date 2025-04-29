namespace DimPos.Catalog.Domain.Models.Product;

public class ProductResponse
{
    public Guid Id { get; set;}
    public string? Code { get; set; }
    public string? AlternativeCode { get; set; }
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
    public string? ColorGroup { get; set; }
    public string? Group { get; set; }
    public string? GroupId { get; set; }
    public bool? IsMenuDisplay { get; set; } = false;
    public int? MaxExtra { get; set; }
    public string? Introduction { get; set; }
    public string? PrintGroup { get; set; }
    public string? WebContent { get; set; }
    public bool? IsDefaultChildProduct { get; set; } = false;
    public int? SaleType { get; set; }
    public bool? IsMostOrdered { get; set; } = false;
    public string? Note { get; set; }
    public int? NumOfUserVoted { get; set; } 
    public Guid? BrandId { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
}