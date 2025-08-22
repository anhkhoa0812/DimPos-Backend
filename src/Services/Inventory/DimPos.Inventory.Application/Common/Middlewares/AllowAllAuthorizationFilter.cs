using Hangfire.Dashboard;

namespace DimPos.Inventory.Application.Common.Middlewares;

public class AllowAllAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        return true;
    }
}