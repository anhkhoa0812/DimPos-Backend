using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);
var sql = builder.AddConnectionString("CatalogDb");


var catalogApi = builder.AddProject<Projects.DimPos_Catalog_Application>("catalog-api")
    .WithReference(sql);
builder.Build().Run();