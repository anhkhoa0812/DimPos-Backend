using DimPos.Catalog.Domain.Models.Common;
using Mediator;

namespace DimPos.Catalog.Application.Features.Categories.Query.GetCategoryById;

public class GetCategoryByIdQuery : IRequest<ApiResponse>
{
    public Guid CategoryId { get; set; }
}