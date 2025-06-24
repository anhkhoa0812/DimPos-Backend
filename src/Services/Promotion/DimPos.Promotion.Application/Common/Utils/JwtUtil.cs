using System.Security.Claims;

namespace DimPos.Promotion.Application.Common.Utils;

public static class JwtUtil
{
    public static string GetCurrentAccountId(ClaimsIdentity identity)
    {
        if (identity != null)
        {
            var userClaims = identity.Claims;
            return userClaims.FirstOrDefault(x => x.Type == "AccountId")?.Value;
        }
        return null;
    }
    public static string GetCurrentEmail(ClaimsIdentity identity)
    {
        if (identity != null)
        {
            var userClaims = identity.Claims;
            return userClaims.FirstOrDefault(x => x.Type == "Email")?.Value;
        }
        return null;
    }
    public static string GetRole(ClaimsIdentity identity)
    {
        if (identity != null)
        {
            var userClaims = identity.Claims;
            return userClaims.FirstOrDefault(x => x.Type == ClaimTypes.Role)?.Value;
        }
        return null;
    }
    public static string GetCurrentUsername(ClaimsIdentity identity)
    {
        if (identity != null)
        {
            var userClaims = identity.Claims;
            return userClaims.FirstOrDefault(x => x.Type == "Username")?.Value;
        }
        return null;
    }
    public static string GetCurrentBrandId(ClaimsIdentity identity)
    {
        if (identity != null)
        {
            var userClaims = identity.Claims;
            return userClaims.FirstOrDefault(x => x.Type == "BrandId")?.Value;
        }
        return null;
    }
    public static string GetCurrentStoreId(ClaimsIdentity identity)
    {
        if (identity != null)
        {
            var userClaims = identity.Claims;
            return userClaims.FirstOrDefault(x => x.Type == "StoreId")?.Value;
        }
        return null;
    }
}