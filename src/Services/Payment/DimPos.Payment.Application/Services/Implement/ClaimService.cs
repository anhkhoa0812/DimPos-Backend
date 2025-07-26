using System.Security.Claims;
using DimPos.Payment.Application.Common.Utils;
using DimPos.Payment.Application.Services.Interface;

namespace DimPos.Payment.Application.Services.Implement;

public class ClaimService : IClaimService
{
    public ClaimService(IHttpContextAccessor httpContextAccessor)
    {
        var identity = httpContextAccessor.HttpContext?.User?.Identity as ClaimsIdentity;
        var accountId = Guid.TryParse(JwtUtil.GetCurrentAccountId(identity), out var accountIdResult ) ? accountIdResult : Guid.Empty;
        var email = JwtUtil.GetCurrentEmail(identity);
        var username = JwtUtil.GetCurrentUsername(identity);
        var role = JwtUtil.GetRole(identity);
        var brandId = Guid.TryParse(JwtUtil.GetCurrentBrandId(identity), out var brandIdResult) ? brandIdResult : (Guid?)null;
        var storeId = Guid.TryParse(JwtUtil.GetCurrentStoreId(identity), out var storeIdResult) ? storeIdResult : (Guid?)null;
        GetCurrentUserId = accountId == Guid.Empty ? Guid.Empty : accountId;
        GetCurrentEmail = string.IsNullOrEmpty(email) ? "" : email;
        GetCurrentUsername = string.IsNullOrEmpty(username) ? "" : username;
        GetBrandId = brandId == Guid.Empty ? Guid.Empty : brandId;
        GetRole = string.IsNullOrEmpty(role) ? string.Empty : role;
        GetStoreId = storeId == Guid.Empty ? Guid.Empty : storeId;
    }
    public Guid GetCurrentUserId { get; }
    public string GetCurrentEmail { get; }
    public string GetCurrentUsername { get; }
    public string GetRole { get; }
    public Guid? GetBrandId { get; }
    public Guid? GetStoreId { get; }
}