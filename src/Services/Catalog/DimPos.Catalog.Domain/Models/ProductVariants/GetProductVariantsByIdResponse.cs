using DimPos.Catalog.Domain.Enums;

namespace DimPos.Catalog.Domain.Models.ProductVariants;

public class GetProductVariantsByIdResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? AlternativeCode { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal? DiscountPercent { get; set; }
    public decimal? DiscountPrice { get; set; }
    public decimal Price { get; set; }
    public decimal? PriceCOGS { get; set; }
    public bool IsActive { get; set; }
    public string? Size { get; set; }
    public bool? IsMenuDisplay { get; set; }
    public EProductVariantStatus Status { get; set; }
    public Guid CategoryId { get; set; }
    public Guid ProductId { get; set; }
}