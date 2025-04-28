using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);



var catalogApi = builder.AddProject<Projects.DimPos_Catalog_Application>("catalog-api");

builder.Build().Run();