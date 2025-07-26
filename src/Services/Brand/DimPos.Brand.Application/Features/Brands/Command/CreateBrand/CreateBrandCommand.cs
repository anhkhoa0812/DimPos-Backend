using DimPos.Brand.Domain.Models.Common;
using Mediator;

namespace DimPos.Brand.Application.Features.Brands.Command.CreateBrand;

public class CreateBrandCommand : IRequest<ApiResponse>
{
    public string Code { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Address { get; set; }
    public string Phone { get; set; }
    public IFormFile? Picture { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
}