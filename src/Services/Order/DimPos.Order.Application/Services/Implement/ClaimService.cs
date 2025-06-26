using System.Security.Claims;
using DimPos.Order.Application.Common.Utils;
using DimPos.Order.Application.Services.Interface;

namespace DimPos.Order.Application.Services.Implement;

public class ClaimService : IClaimService
{
    public ClaimService(IHttpContextAccessor httpContextAccessor)
    {
        var identity = httpContextAccessor.HttpContext?.User?.Identity as ClaimsIdentity;
        var accountId = Guid.TryParse(JwtUtil.GetCurrentAccountId(identity), out var accountIdResult ) ? accountIdResult : Guid.Empty;
        var email = JwtUtil.GetCurrentEmail(identity);
        var username = JwtUtil.GetCurrentUsername(identity);
        var role = JwtUtil.GetRole(identity);
        var brandId = Guid.TryParse(JwtUtil.GetCurrentBrandId(identity), out var brandIdResult) ? brandIdResult : Guid.Empty;
        var storeId = Guid.TryParse(JwtUtil.GetCurrentStoreId(identity), out var storeIdResult) ? storeIdResult : Guid.Empty;
        GetCurrentUserId = accountId;
        GetCurrentEmail = string.IsNullOrEmpty(email) ? "" : email;
        GetCurrentUsername = string.IsNullOrEmpty(username) ? "" : username;
        GetBrandId = brandId;
        GetRole = string.IsNullOrEmpty(role) ? string.Empty : role;
        GetStoreId = storeId;
    }
    public Guid GetCurrentUserId { get; }
    public string GetCurrentEmail { get; }
    public string GetCurrentUsername { get; }
    public string GetRole { get; }
    public Guid? GetBrandId { get; }
    public Guid? GetStoreId { get; }
}