namespace DimPos.Catalog.Domain.Models.Ingredients;

public class GetIngredientsByBrandResponse
{
    public Guid Id { get; set; }
    public string? Code { get; set; }
    public string? Sku { get; set; }
    public string Name { get; set; } = string.Empty;
    public string MeasureUnit { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}