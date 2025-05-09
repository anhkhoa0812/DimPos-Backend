namespace DimPos.Identity.Domain.Models.Authentication;

public class LoginResponse
{
    public Guid AccountId { get; set; }
    public string Username { get; set; }
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
}