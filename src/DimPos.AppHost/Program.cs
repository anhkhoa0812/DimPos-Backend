var builder = DistributedApplication.CreateBuilder(args);
var sql = builder.AddConnectionString("CatalogDb");
// var kafka = builder
//     .AddKafka("kafka", 9092)
//     .WithEnvironment("KAFKA_SASL_USERNAME", "kafka")
//     .WithEnvironment("KAFKA_SASL_PASSWORD", "kafka")
//     .WithEnvironment("KAFKA_SASL_MECHANISM", "PLAIN")
//     .WithKafkaUI()
//     .WithLifetime(ContainerLifetime.Persistent)
//     .WithDataVolume();

var catalogApi = builder.AddProject<Projects.DimPos_Catalog_Application>("catalog-api")
    .WithReference(sql);
var identityApi = builder.AddProject<Projects.DimPos_Identity_Application>("identity-api");

var brandApi = builder.AddProject<Projects.DimPos_Brand_Application>("brand-api");
    // .WithReference(kafka)
    // .WaitFor(kafka);
var storeApi = builder.AddProject<Projects.DimPos_Store_Application>("store-api");
var menuComboApi = builder.AddProject<Projects.DimPos_MenuCombo_Application>("menu-combo-api");
var mediaApi = builder.AddProject<Projects.DimPos_Media_Application>("media-api");
var orchestrator = builder.AddProject<Projects.DimPos_Orchestrator>("orchestrator")
        .WithReference(brandApi)
        .WithReference(catalogApi)
        .WithReference(identityApi)
        .WithReference(storeApi);

builder.Build().Run();