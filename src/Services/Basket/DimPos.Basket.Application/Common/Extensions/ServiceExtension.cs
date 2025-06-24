using System.Text;
using DimPos.Basket.Application.Models.Setting;
using DimPos.Basket.Application.Services;
using DimPos.Basket.Application.Services.Interface;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using StackExchange.Redis;

namespace DimPos.Basket.Application.Common.Extensions;

public static class ServiceExtension
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IRedisService, RedisService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IClaimService, ClaimService>();
        return services;
    }
    public static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConnectionString = configuration.GetConnectionString("Redis");
        if (redisConnectionString != null)
            services.AddSingleton<IConnectionMultiplexer>(
                ConnectionMultiplexer.Connect(redisConnectionString));
        return services;
    }
    public static void AddJWT(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JWTSettings").Get<JwtSettings>();

        services.AddAuthentication(opt =>
        {
            opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidIssuer = jwtSettings?.Issuer,
                ValidAudience = jwtSettings?.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings?.SecurityKey!)),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true
            };
        });
    }
    public static void AddOpenApiConfig(this IServiceCollection services)
    {
        services.AddOpenApi(opt =>
        {
            opt.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                document.Info.Title = "Baskets";
                document.Info.Contact = new OpenApiContact()
                {
                    Email = "tahoanganhkhoa2014@gmail.com",
                    Name = "DimPos - Ta Hoang Anh Khoa"
                };

                return Task.CompletedTask;
            });
            opt.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
        });
    }
        
    internal sealed class BearerSecuritySchemeTransformer(
        IAuthenticationSchemeProvider authenticationSchemeProvider
    ) : IOpenApiDocumentTransformer
    {
        public async Task TransformAsync(
            OpenApiDocument document,
            OpenApiDocumentTransformerContext context,
            CancellationToken cancellationToken
        )
        {
            var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();
            if (authenticationSchemes.Any(authScheme => authScheme.Name == "Bearer"))
            {
                var requirements = new Dictionary<string, OpenApiSecurityScheme>
                {
                    ["Bearer"] = new OpenApiSecurityScheme
                    {
                        In = ParameterLocation.Header,
                        Description = "Please enter a valid token using the Bearer scheme (\"bearer {token}\")",
                        Name = "Authorization",
                        Type = SecuritySchemeType.Http,
                        BearerFormat = "Security",
                        Scheme = "Bearer"
                    },
                };
                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes = requirements;
                foreach (var operation in document.Paths.Values.SelectMany(path => path.Operations))
                {
                    operation.Value.Security.Add(new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecurityScheme { Reference = new OpenApiReference { Id = "Bearer", Type = ReferenceType.SecurityScheme } }] = Array.Empty<string>()
                    });
                }
            }
        }
    }

    public static void UseScalar(this WebApplication app)
    {

        app.MapOpenApi();
        app.MapScalarApiReference(options =>
            {
                options.EndpointPathPrefix = "/api/{documentName}";
                options.Theme = ScalarTheme.DeepSpace;
                options.Favicon = "/assets/images/dimposlogo.png";
            })
            .RequireAuthorization(options => { options.RequireAssertion(context => { return true; }); });
    }
}