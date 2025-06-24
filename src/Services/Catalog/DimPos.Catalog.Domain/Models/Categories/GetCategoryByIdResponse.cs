using DimPos.Catalog.Domain.Enums;

namespace DimPos.Catalog.Domain.Models.Categories;

public record GetCategoryByIdResponse
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
    public ParentCategoryResponse? ParentCategory { get; set; }
}
public record ParentCategoryResponse
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