namespace DimPos.Store.Domain.Models.Response;

public class StaffDetailsResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = String.Empty;
    public string Username { get; set; } = String.Empty;
    public string Email { get; set; } = String.Empty;
}