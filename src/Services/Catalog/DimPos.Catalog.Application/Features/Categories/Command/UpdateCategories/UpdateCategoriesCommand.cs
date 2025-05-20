using DimPos.Catalog.Domain.Enums;
using DimPos.Catalog.Domain.Models.Common;
using Mediator;

namespace DimPos.Catalog.Application.Features.Categories.Command.UpdateCategories;

public class UpdateCategoriesCommand : IRequest<ApiResponse>
{
    public Guid CategoryId { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public int? DisplayOrder { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public ECategoryStatus Status { get; set; }
    public IFormFile? Image { get; set; }
}

public class UpdateCategoriesRequest
{
    public string Name { get; set; }
    public string? Description { get; set; }
    public int? DisplayOrder { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public ECategoryStatus Status { get; set; }
    public IFormFile? Image { get; set; }
}