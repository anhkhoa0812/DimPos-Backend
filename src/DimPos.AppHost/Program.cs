var builder = DistributedApplication.CreateBuilder(args);
// var kafka = builder
//     .AddKafka("kafka", 9092)
//     .WithEnvironment("KAFKA_SASL_USERNAME", "kafka")
//     .WithEnvironment("KAFKA_SASL_PASSWORD", "kafka")
//     .WithEnvironment("KAFKA_SASL_MECHANISM", "PLAIN")
//     .WithEnvironment("KAFKA_CFG_SOCKET_REQUEST_MAX_BYTES", "524288000")
//     .WithEnvironment("KAFKA_CFG_MESSAGE_MAX_BYTES", "524288000")
//     .WithKafkaUI()
//     .WithLifetime(ContainerLifetime.Persistent)
//     .WithDataVolume();
//
var catalogApi = builder.AddProject<Projects.DimPos_Catalog_Application>("catalog-api");
var identityApi = builder.AddProject<Projects.DimPos_Identity_Application>("identity-api");

var brandApi = builder.AddProject<Projects.DimPos_Brand_Application>("brand-api");

var storeApi = builder.AddProject<Projects.DimPos_Store_Application>("store-api");

var menuComboApi = builder.AddProject<Projects.DimPos_MenuCombo_Application>("menu-combo-api");

var mediaApi = builder.AddProject<Projects.DimPos_Media_Application>("media-api");

var basketApi = builder.AddProject<Projects.DimPos_Basket_Application>("basket-api");
var promotionApi = builder.AddProject<Projects.DimPos_Promotion_Application>("promotion-api");
var orderApi = builder.AddProject<Projects.DimPos_Order_Application>("order-api");
var paymentApi = builder.AddProject<Projects.DimPos_Payment_Application>("payment-api");
var inventoryApi = builder.AddProject<Projects.DimPos_Inventory_Application>("inventory-api");
var notificationApi = builder.AddProject<Projects.DimPos_Notification_Application>("notification-api");
var orchestrator = builder.AddProject<Projects.DimPos_Orchestrator>("orchestrator")
        .WithReference(brandApi)
        .WithReference(catalogApi)
        .WithReference(identityApi)
        .WithReference(storeApi);

builder.Build().Run();