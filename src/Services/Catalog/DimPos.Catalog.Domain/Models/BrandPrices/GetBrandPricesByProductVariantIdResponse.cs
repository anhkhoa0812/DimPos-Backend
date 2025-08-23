namespace DimPos.Catalog.Domain.Models.BrandPrices;

public class GetBrandPriceHistoriesByProductVariantIdResponse
{
    public Guid Id { get; set; }
    public string CurrencyCode { get; set; }
    public decimal OldPrice { get; set; }
    public decimal NewPrice { get; set; }
    public DateTime ChangedAt { get; set; }
    public Guid ChangedBy { get; set; }
    public Guid ProductVariantId { get; set; }
}