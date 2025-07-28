using DimPos.Catalog.Domain.Models.Common;
using Mediator;

namespace DimPos.Catalog.Application.Features.ProductVariants.Command.UpdateInactiveForProductVariants;

public class UpdateInactiveForProductVariantsCommand : IRequest<ApiResponse>
{
    public Guid ProductId { get; set; }
}