using DimPos.Store.Domain.Enums;
using DimPos.Store.Domain.Models.Common;
using Mediator;

namespace DimPos.Store.Application.Features.Stores.Command.UpdateStaff;

public class UpdateStaffCommand : IRequest<ApiResponse>
{
    public Guid StaffId { get; set; }
    public string? Code { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? Email { get; set; }
    public EAccountStatus? Status { get; set; }
}

public class UpdateStaffRequest
{
    public string? Code { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }

    public string? Email { get; set; }
    public EAccountStatus? Status { get; set; }
}