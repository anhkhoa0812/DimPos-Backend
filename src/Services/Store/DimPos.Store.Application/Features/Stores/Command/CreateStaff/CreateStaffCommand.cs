using DimPos.Store.Domain.Models.Common;
using Mediator;

namespace DimPos.Store.Application.Features.Stores.Command.CreateStaff;

public class CreateStaffCommand : IRequest<ApiResponse>
{
    public string Code { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string? Email { get; set; }
    
}