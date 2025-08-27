using Carter;
using DimPos.Order.Application.Features.Dashboards.Query.ExportExcelForDashboardBrand;
using DimPos.Order.Application.Features.Dashboards.Query.ExportExcelForDashboardStore;
using DimPos.Order.Application.Features.Dashboards.Query.GetDashboardForBrand;
using DimPos.Order.Application.Features.Dashboards.Query.GetDashboardForStore;
using DimPos.Order.Domain.Constants;
using DimPos.Order.Domain.Models.Common;
using DimPos.Order.Domain.Models.Response;
using Grpc.Core;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace DimPos.Order.Application.Endpoints;

public class DashboardEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(ApiEndpointConstants.Dashboards.DashboardsEndpoint).WithTags("Dashboards");
        group.MapGet("/brands", GetDashboardForBrand)
            .WithName(nameof(GetDashboardForBrand))
            .RequireAuthorization("BrandPolicy")
            .Produces<ApiResponse<DashboardResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapGet("/stores", GetDashboardForStore)
            .WithName(nameof(GetDashboardForStore))
            .RequireAuthorization("StorePolicy")
            .Produces<ApiResponse<DashboardResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapGet("/brands/export", ExportExcelForBrandDashboard)
            .WithName(nameof(ExportExcelForBrandDashboard))
            .RequireAuthorization("BrandPolicy")
            .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapGet("/stores/export", ExportExcelForStoreDashboard)
            .WithName(nameof(ExportExcelForStoreDashboard))
            .RequireAuthorization("StorePolicy")
            .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
    }
    
    public async Task<IResult> GetDashboardForBrand(IMediator mediator, [FromQuery] Guid? storeId, 
        [FromQuery] DateOnly fromDate, [FromQuery] DateOnly toDate)
    {
        var query = new GetDashboardForBrandQuery()
        {
            StoreId = storeId,
            FromDate = fromDate,
            ToDate = toDate
        };
        var apiResponse = await mediator.Send(query);
        return Results.Ok(apiResponse);
    }
    public async Task<IResult> GetDashboardForStore(IMediator mediator, [FromQuery] DateOnly fromDate, 
        [FromQuery] DateOnly toDate)
    {
        var query = new GetDashboardForStoreQuery()
        {
            FromDate = fromDate,
            ToDate = toDate
        };
        var apiResponse = await mediator.Send(query);
        return Results.Ok(apiResponse);
    }
    public async Task<IResult> ExportExcelForBrandDashboard(IMediator mediator, [FromQuery] DateOnly fromDate, 
        [FromQuery] DateOnly toDate)
    {
        var query = new ExportExcelForDashboardBrandQuery()
        {
            FromDate = fromDate,
            ToDate = toDate
        };
        var apiResponse = await mediator.Send(query);
        return Results.Ok(apiResponse);
    }
    public async Task<IResult> ExportExcelForStoreDashboard(IMediator mediator, [FromQuery] DateOnly fromDate,
        [FromQuery] DateOnly toDate)
    {
        var query = new ExportExcelForDashboardStoreQuery()
        {
            FromDate = fromDate,
            ToDate = toDate
        };
        var apiResponse = await mediator.Send(query);
        return Results.Ok(apiResponse);
    }
}