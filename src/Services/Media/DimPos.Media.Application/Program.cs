using DimPos.Media.Application.GrpcService;
using DimPos.Media.Application.Service.Implement;
using DimPos.Media.Application.Service.Interface;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddCors();
builder.Services.AddGrpc();
builder.Services.AddScoped<IUploadService, UploadService>();
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
app.MapGrpcService<MediaGrpcService>();
app.Run();
