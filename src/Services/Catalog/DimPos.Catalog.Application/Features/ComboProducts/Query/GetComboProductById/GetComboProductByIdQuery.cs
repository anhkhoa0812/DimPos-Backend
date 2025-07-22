using DimPos.Catalog.Domain.Models.Common;
using Mediator;

namespace DimPos.Catalog.Application.Features.ComboProducts.Query.GetComboProductById;

public class GetComboProductByIdQuery : IRequest<ApiResponse>
{
    public Guid ProductVariantId { get; set; }
}