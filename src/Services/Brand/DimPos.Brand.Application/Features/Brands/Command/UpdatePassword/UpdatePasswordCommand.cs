using DimPos.Brand.Domain.Models.Common;
using Mediator;

namespace DimPos.Brand.Application.Features.Brands.Command.UpdatePassword;

public class UpdatePasswordCommand : IRequest<ApiResponse>
{
    public Guid BrandId { get; set; }
    public string Password { get; set; } = String.Empty;
}

public class UpdatePasswordRequest
{
    public string Password { get; set; } = String.Empty;
}