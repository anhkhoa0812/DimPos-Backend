using DimPos.Media.Application.Common.Models.Settings;
using DimPos.Media.Application.GrpcService;
using DimPos.Media.Application.Service.Implement;
using DimPos.Media.Application.Service.Interface;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddCors();
builder.Services.AddGrpc();
builder.Services.AddScoped<IUploadService, UploadService>();
builder.Services.Configure<S3CompatibleStorageSettings>(builder.Configuration.GetSection("S3CompatibleStorageSettings"));
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors(builder =>
    builder.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
app.MapGet("/proxy/{**url}", async ([FromRoute] string url, [FromServices] IHttpClientFactory httpClientFactory, HttpContext context) =>
{
    if (!url.StartsWith("http"))
    {
        return Results.BadRequest("Invalid URL");
    }

    var client = httpClientFactory.CreateClient();

    try
    {
        var response = await client.GetAsync(url);

        // Copy status code
        context.Response.StatusCode = (int)response.StatusCode;

        // Copy headers
        foreach (var header in response.Headers)
        {
            context.Response.Headers[header.Key] = header.Value.ToArray();
        }
        foreach (var header in response.Content.Headers)
        {
            context.Response.Headers[header.Key] = header.Value.ToArray();
        }

        // Copy body
        var stream = await response.Content.ReadAsStreamAsync();
        await stream.CopyToAsync(context.Response.Body);

        return Results.Empty;
    }
    catch
    {
        return Results.StatusCode(500);
    }
});
app.MapGrpcService<MediaGrpcService>();
app.Run();
