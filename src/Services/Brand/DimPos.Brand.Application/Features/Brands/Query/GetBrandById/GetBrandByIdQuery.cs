using DimPos.Brand.Domain.Models.Common;
using Mediator;

namespace DimPos.Brand.Application.Features.Brands.Query.GetBrandById;

public class GetBrandByIdQuery : IRequest<ApiResponse>
{
    public Guid BrandId { get; set; }
}