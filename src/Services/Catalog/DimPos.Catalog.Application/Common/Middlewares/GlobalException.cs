using System.Net;
using System.Text.Json;
using DimPos.Catalog.Domain.Models.Common;
namespace DimPos.Catalog.Application.Common.Middlewares;

public class GlobalException
{
    private readonly RequestDelegate _next;
    private readonly ILogger _logger;
    public GlobalException(RequestDelegate next, ILogger logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        var response = context.Response;
        var errorResponse = new ApiResponse();
        switch (exception)
        {
            //add more custom exception
            //For example case AppException: do something
            case BadHttpRequestException badRequestException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.Message = "Bad request.";
                _logger.Error(badRequestException, "Bad request error");
                break;
            default:
                //unhandled error
                response.StatusCode = (int) HttpStatusCode.InternalServerError;
                errorResponse.Status = HttpStatusCode.InternalServerError;
                errorResponse.Message = "An unexpected error occurred.";
                _logger.Error(exception, "Unhandled exception");
                break;
        }
        var result = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(result);
    }
}