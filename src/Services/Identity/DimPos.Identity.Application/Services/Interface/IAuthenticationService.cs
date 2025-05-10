using DimPos.Identity.Domain.Entities;

namespace DimPos.Identity.Application.Services.Interface;

public interface IAuthenticationService
{
    string GenerateAccessToken(Accounts accounts, string? brandId);
    string GenerateRefreshToken();
}