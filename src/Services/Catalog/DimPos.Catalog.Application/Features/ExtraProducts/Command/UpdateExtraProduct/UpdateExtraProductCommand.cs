using DimPos.Catalog.Domain.Models.Common;
using Mediator;

namespace DimPos.Catalog.Application.Features.ExtraProducts.Command.UpdateExtraProduct;

public class UpdateExtraProductCommand : IRequest<ApiResponse>
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int? DisplayOrder { get; set; }
    public bool? IsActive { get; set; }
    public decimal? Price { get; set; }
}
public class UpdateExtraProductRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int? DisplayOrder { get; set; }
    public bool? IsActive { get; set; }
    public decimal? Price { get; set; }
}