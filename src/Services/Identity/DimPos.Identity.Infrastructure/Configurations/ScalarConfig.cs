using Microsoft.AspNetCore.Builder;
using Scalar.AspNetCore;

namespace DimPos.Identity.Infrastructure.Configurations;

public static class ScalarConfig
{
    public static void UseScalar(this WebApplication app)
    {

        app.MapOpenApi();
        app.MapScalarApiReference(options =>
            {
                options.EndpointPathPrefix = "/api/{documentName}";
                options.Theme = ScalarTheme.DeepSpace;
                options.Favicon = "/assets/images/dimposlogo.png";
                options.Title = "Identity";
            })
            .RequireAuthorization(options =>
            {
                options.RequireAssertion(context =>
                {
                    return true;
                });
            });
    }
}