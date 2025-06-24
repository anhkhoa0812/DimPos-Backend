using DimPos.MenuCombo.Domain.Models.Common;
using Mediator;

namespace DimPos.MenuCombo.Application.Features.BrandMenu.Query.GetBrandMenuById;

public class GetBrandMenuByIdQuery : IRequest<ApiResponse>
{
    public Guid BrandMenuId { get; set; }
}