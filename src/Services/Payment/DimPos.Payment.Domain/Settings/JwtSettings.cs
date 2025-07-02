namespace DimPos.Payment.Domain.Settings;

public class JwtSettings
{
    public string SecurityKey { get; set; } = null!;
    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;
    public int TokenExpiry { get; set; }
}