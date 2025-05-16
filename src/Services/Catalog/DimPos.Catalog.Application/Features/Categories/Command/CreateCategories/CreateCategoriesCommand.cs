using DimPos.Catalog.Domain.Models.Common;
using Mediator;

namespace DimPos.Catalog.Application.Features.Categories.Command.CreateCategories;

public class CreateCategoriesCommand : IRequest<ApiResponse>
{
    public string? Code { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public string Type { get; set; }
    public int? DisplayOrder { get; set; }
    public IFormFile? Image { get; set; }
    public int Status { get; set; }
    
    public Guid? ParentId { get; set; }
}