using DimPos.Catalog.Domain.Models.Common;
using Mediator;

namespace DimPos.Catalog.Application.Features.Test;

public class GetMenuStoreQuery : IRequest<ApiResponse>
{
    public Guid BrandId { get; set; }
    public Guid StoreId { get; set; }
    public List<Guid> ProductVariantIds { get; set; } = new();
}