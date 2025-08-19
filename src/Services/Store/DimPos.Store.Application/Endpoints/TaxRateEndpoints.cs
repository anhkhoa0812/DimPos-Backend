using Carter;
using DimPos.Store.Application.Features.TaxRate.Query.GetTaxRateById;
using DimPos.Store.Domain.Constants;
using DimPos.Store.Domain.Models.Common;
using DimPos.Store.Domain.Models.Response;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace DimPos.Store.Application.Endpoints;

public class TaxRateEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(ApiEndpointConstant.TaxRate.TaxRateEndpoint).WithTags("Tax Rate");
        group.MapGet("{id:guid}", GetTaxRateById)
            .WithName(nameof(GetTaxRateById))
            .RequireAuthorization("StoreStaffAndBrandPolicy")
            .Produces<ApiResponse<GetTaxRateByIdResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
    }
    
    public async Task<IResult> GetTaxRateById(IMediator mediator, [FromRoute] Guid id)
    {
        var query = new GetTaxRateByIdQuery() { TaxRateId = id };
        var response = await mediator.Send(query);
        return Results.Ok(response);
    }
}