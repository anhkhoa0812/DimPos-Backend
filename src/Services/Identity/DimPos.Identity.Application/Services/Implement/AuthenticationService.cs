using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DimPos.Identity.Application.Services.Interface;
using DimPos.Identity.Domain.Entities;
using DimPos.Identity.Domain.Enum;
using DimPos.Identity.Domain.Models.Settings;
using DimPos.Identity.Infrastructure.Configurations;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DimPos.Identity.Application.Services.Implement;

public class AuthenticationService : IAuthenticationService
{
    private readonly JwtSettings _jwtSettings;

    public AuthenticationService(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }

    public string GenerateAccessToken(Accounts accounts,  ERoleName roleName, string? brandId, string? storeId)
    {
        var tokenhandler = new JwtSecurityTokenHandler();
        var tokenkey = Encoding.UTF8.GetBytes(_jwtSettings.SecurityKey!);
        var timeExpire = DateTime.UtcNow.AddDays((double)_jwtSettings.TokenExpiry!);
        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(
                new Claim[]
                {
                    new Claim("AccountId", accounts.Id.ToString()),
                    new Claim("Email", accounts.Email ?? string.Empty),
                    new Claim("Username", accounts.Username),
                    new Claim(ClaimTypes.Role, roleName.ToString()),
                    !string.IsNullOrEmpty(brandId) ? new Claim("BrandId", brandId) : new Claim("BrandId", string.Empty),
                    !string.IsNullOrEmpty(storeId) ? new Claim("StoreId", storeId) : new Claim("StoreId", string.Empty),
                }
            ),
            Expires = timeExpire,
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenkey), SecurityAlgorithms.HmacSha256)
        };
        var token = tokenhandler.CreateToken(tokenDescriptor);
        var tokenString = tokenhandler.WriteToken(token);
        return tokenString;
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}