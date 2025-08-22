using DimPos.Brand.Domain.Models.Common;
using Mediator;

namespace DimPos.Brand.Application.Features.Brands.Command.UpdateBrands;

public class UpdateBrandsCommand : IRequest<ApiResponse>
{
    public string? Name { get; set; } 
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public IFormFile? Picture { get; set; }
}
