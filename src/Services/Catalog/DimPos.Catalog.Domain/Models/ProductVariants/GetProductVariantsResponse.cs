using DimPos.Catalog.Domain.Enums;

namespace DimPos.Catalog.Domain.Models.ProductVariants;

public class GetProductVariantsResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty; 
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
    public string? Size { get; set; }
    public string? Sku { get; set; }
}