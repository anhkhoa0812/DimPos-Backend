using DimPos.MenuCombo.Domain.Enums;

namespace DimPos.MenuCombo.Domain.Models.ProductVariant;

public record ProductVariantResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; }
    public string? AlternativeCode { get; set; }
    public string Name { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal DiscountPrice { get; set; }
    public decimal Price { get; set; }
    public decimal PriceCOGS { get; set; }
    public bool IsActive { get; set; }
    public string? Size { get; set; }
    public bool IsMenuDisplay { get; set; }
    public int DisplayOrder { get; set; }
    public EProductVariantStatus Status { get; set; }
    public bool IsSelected { get; set; }
    public string? Sku { get; set; }
}