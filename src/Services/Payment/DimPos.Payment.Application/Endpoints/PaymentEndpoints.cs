using Carter;
using DimPos.Payment.Application.Services.Interface;
using DimPos.Payment.Domain.Models.Common;
using DimPos.Payment.Domain.Models.MPos.Base;
using DimPos.Payment.Domain.Models.Payment;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;

namespace DimPos.Payment.Application.Endpoints;

public class PaymentEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/payment").WithTags("Payment");
        group.MapPost("/qr", CreateQr)
            .WithName(nameof(CreateQr));
        group.MapPost("/qr/cancel", CancelQrPayment)
            .WithName(nameof(CancelQrPayment));
        group.MapGet("/qr/status", GetQrStatus)
            .WithName(nameof(GetQrStatus));
        group.MapPost("/edc", CreateEDCPayment)
            .WithName(nameof(CreateEDCPayment));
        group.MapPost("/edc/cancel", CancelEDCPayment)
            .WithName(nameof(CancelEDCPayment));
        group.MapGet("/edc/status", GetEDCStatus)
            .WithName(nameof(GetEDCStatus));
        group.MapPost("/edc/refund", RefundEDCPayment)
            .WithName(nameof(RefundEDCPayment));
        group.MapPost("/callback", Callback)
            .DisableAntiforgery()
            .WithName(nameof(Callback)); 
    }

    public async Task<IResult> CreateQr(IMPosService service, [FromBody] CreateQrPaymentRequest request)
    {
        var apiResponse = await service.CreateQr(request);
        return Results.Json(apiResponse);
    }
    public async Task<IResult> CreateEDCPayment(IMPosService service, [FromBody] CreateEDCPaymentRequest request)
    {
        var apiResponse = await service.CreateEDCPayment(request);
        return Results.Json(apiResponse);
    }

    public async Task<IResult> CancelEDCPayment(IMPosService service, [FromBody] CreateCancelEDCRequest request)
    {
        var apiResponse = await service.CancelEDCPayment(request);
        return Results.Json(apiResponse);
    }

    public async Task<IResult> CancelQrPayment(IMPosService service, [FromBody] CreateCancelQrRequest request)
    {
        var apiResponse = await service.CancelQrPayment(request);
        return Results.Json(apiResponse);
    }
    public async Task<IResult> GetQrStatus(IMPosService service, [FromQuery] Guid orderId, [FromQuery] string credentialsConfig, [FromQuery] string key)
    {
        var request = new GetQrStatusRequest()
        {
            OrderId = orderId,
            CredentialsConfig = credentialsConfig,
            Key = key
        };
        var apiResponse = await service.GetQrStatus(request);
        return Results.Json(apiResponse);
    }

    public async Task<IResult> GetEDCStatus(IMPosService service, [FromBody] GetEDCStatusRequest request)
    {
        var apiResponse = await service.GetEDCStatus(request);
        return Results.Json(apiResponse);
    }
    public async Task<IResult> RefundEDCPayment(IMPosService service, [FromBody] GetRefundEDCPaymentRequest request)
    {
        var apiResponse = await service.GetRefundEDCPayment(request);
        return Results.Json(apiResponse);
    }

    public async Task<IResult> Callback(IMPosService service, [FromBody] MPosRequest request)
    {
        await service.HandleMPosCallback(request);
        var response =  new ApiResponse()
        {
            Data = StatusCodes.Status200OK,
            Message = "Callback processed successfully",
        };
        return Results.Ok(response);
    }
}