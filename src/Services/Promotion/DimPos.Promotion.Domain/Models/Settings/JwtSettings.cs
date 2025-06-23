namespace DimPos.Promotion.Domain.Models.Settings;

public class JwtSettings
{
    public string SecurityKey { get; set; } = null!;
    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;
    public int TokenExpiry { get; set; }
}