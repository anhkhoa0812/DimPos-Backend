using DimPos.Identity.Domain.Entities;
using DimPos.Identity.Domain.Enum;

namespace DimPos.Identity.Application.Services.Interface;

public interface IAuthenticationService
{
    string GenerateAccessToken(Accounts accounts,  ERoleName roleName, string? brandId);
    string GenerateRefreshToken();
}