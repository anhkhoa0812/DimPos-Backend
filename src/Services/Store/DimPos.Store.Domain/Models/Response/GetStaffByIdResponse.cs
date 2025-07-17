using DimPos.Store.Domain.Enums;

namespace DimPos.Store.Domain.Models.Response;

public class GetStaffByIdResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? Email { get; set; }
    public EAccountStatus Status { get; set; }
    public DateTime AssignAt { get; set; }
}