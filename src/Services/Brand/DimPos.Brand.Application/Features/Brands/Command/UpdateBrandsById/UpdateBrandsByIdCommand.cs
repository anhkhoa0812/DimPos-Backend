using DimPos.Brand.Domain.Models.Common;
using Mediator;

namespace DimPos.Brand.Application.Features.Brands.Command.UpdateBrandsById;

public class UpdateBrandsByIdCommand : IRequest<ApiResponse>
{
    public Guid BrandId { get; set; }
    public string? Name { get; set; } 
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public IFormFile? Picture { get; set; }
}

public class UpdateBrandsByIdRequest
{
    public string? Name { get; set; } 
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public IFormFile? Picture { get; set; }
}