using DimPos.Identity.Domain.Models.Common;
using Mediator;

namespace DimPos.Identity.Application.Features.Authentication.Command.Login;

public class LoginCommand : IRequest<ApiResponse>
{
    public string Username { get; set; }
    public string Password { get; set; }
}