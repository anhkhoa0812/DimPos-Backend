namespace DimPos.Catalog.Domain.Models.Categories;

public class CategoriesResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public string Description { get; set; }
    public string Type { get; set; }
    public int? DisplayOrder { get; set; }
    public string PictureUrl { get; set; }
    public bool? HasChildCategory { get; set; } = false;
    public int? Status { get; set; } = 1;
}