using DimPos.Catalog.Domain.Enums;

namespace DimPos.Catalog.Domain.Models.Product;

public class ProductByIdResponse : ProductResponse
{
    public CategoryResponse Category { get; set; } = new CategoryResponse();
}

public record CategoryResponse
{
    public Guid Id { get; set; }
    public string? Code { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ECategoryType Type { get; set; }
    public int? DisplayOrder { get; set; }
    public string? PictureUrl { get; set; }
    public bool HasChildCategory { get; set; }
    public ECategoryStatus Status { get; set; }
}