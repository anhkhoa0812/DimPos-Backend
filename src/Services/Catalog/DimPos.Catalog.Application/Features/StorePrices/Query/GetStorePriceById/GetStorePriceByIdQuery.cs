using DimPos.Catalog.Domain.Models.Common;
using Mediator;

namespace DimPos.Catalog.Application.Features.StorePrices.Query.GetStorePriceById;

public class GetStorePriceByIdQuery : IRequest<ApiResponse>
{
    public Guid StorePriceId { get; set; }
}