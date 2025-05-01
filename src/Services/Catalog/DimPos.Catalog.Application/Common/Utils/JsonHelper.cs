using System.Text.Json;

namespace DimPos.Catalog.Application.Common.Utils;

public static class JsonHelper
{
    private static readonly JsonSerializerOptions _snakeCaseOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public static IResult Json(object data)
    {
        return Results.Json(data, _snakeCaseOptions, statusCode:200);
    }
}