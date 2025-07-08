using Confluent.Kafka;
using DimPos.Catalog.Domain.Entities;
using DimPos.Catalog.Infrastructure.Persistence;
using DimPos.Catalog.Infrastructure.Repositories.Interface;
using DimPos.Catalog.Infrastructure.Utils;
using MassTransit;
using SharedProject.Events.AssignMenuForStore;

namespace DimPos.Catalog.Application.Consumers;

public class AddStorePriceRequestConsumer : IConsumer<AddStorePriceRequestModel>
{
    private readonly IUnitOfWork<CatalogContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly ITopicProducer<Null, AddStorePriceResponseModel> _successTopicProducer;
    private readonly ITopicProducer<Null, AddStorePriceErrorModel> _failureTopicProducer;
    public AddStorePriceRequestConsumer(IUnitOfWork<CatalogContext> unitOfWork, ILogger logger,
        ITopicProducer<Null, AddStorePriceResponseModel> successTopicProducer,
        ITopicProducer<Null, AddStorePriceErrorModel> failureTopicProducer)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _successTopicProducer = successTopicProducer ?? throw new ArgumentNullException(nameof(successTopicProducer));
        _failureTopicProducer = failureTopicProducer ?? throw new ArgumentNullException(nameof(failureTopicProducer));
    }
    
    public async Task Consume(ConsumeContext<AddStorePriceRequestModel> context)
    {
        try
        {
            _logger.Information("AddStorePriceRequestConsumer: {CorrelationId}", context.Message.CorrelationId);
            var productVariantIds = context.Message.StorePrices
                .Select(x => x.ProductVariantId)
                .Distinct()
                .ToList();
            var brandPrices = await _unitOfWork.GetRepository<BasePrice>().GetListAsync(
                predicate: x => x.BrandId == context.Message.BrandId 
                            && productVariantIds.Contains(x.ProductVariantId)
            );
            var basePriceDictionary = brandPrices.ToDictionary(x => x.ProductVariantId, x => x);
            
            var storePrices = new List<StorePrice>();
            foreach (var storePriceRequest in context.Message.StorePrices)
            {
                if (!basePriceDictionary.TryGetValue(storePriceRequest.ProductVariantId, out var brandPrice))
                    throw new BadHttpRequestException("Không tìm thấy giá của sản phẩm trong danh sách giá của thương hiệu");
                
                var newStorePrice = new StorePrice()
                {
                    Id = Guid.CreateVersion7(),
                    StoreId = storePriceRequest.StoreId,
                    ProductVariantId = storePriceRequest.ProductVariantId,
                    CurrencyCode = "VND",
                    OverridePrice = brandPrice.Price,
                    IsActiveAtStore = true,
                    EffectiveFrom = TimeUtil.GetCurrentSEATime(),
                    StorePriceHistories = new List<StorePriceHistory>()
                    {
                        new()
                        {
                            Id = Guid.CreateVersion7(),
                            CurrencyCode = "VND",
                            NewPrice = brandPrice.Price,
                            OldPrice = 0,
                            ChangedAt = TimeUtil.GetCurrentSEATime(),
                            ChangedBy = context.Message.BrandId,
                            StoreId = storePriceRequest.StoreId,
                            ProductVariantId = storePriceRequest.ProductVariantId,
                        }
                    }
                };
                storePrices.Add(newStorePrice);
            }
            await _unitOfWork.GetRepository<StorePrice>().InsertRangeAsync(storePrices);
            var isSuccess = await _unitOfWork.CommitAsync() > 0;
            if (isSuccess)
            {
                var addStorePriceResponseModel = new AddStorePriceResponseModel()
                {
                    CorrelationId = context.Message.CorrelationId,
                };
                await _successTopicProducer.Produce(
                    key: null,
                    addStorePriceResponseModel,
                    context.CancellationToken
                ).ConfigureAwait(false);
                _logger.Information("AddStorePriceRequestConsumer: {CorrelationId} - Success", context.Message.CorrelationId);
            }
            else
            {
                var storeIds = context.Message.StorePrices.Select(x => x.ProductVariantId)
                    .Distinct()
                    .ToList();
                await ProduceError(_failureTopicProducer, context, storeIds);
                _logger.Error("AddStorePriceRequestConsumer: {CorrelationId} - Failed", context.Message.CorrelationId);
            }
        }
        catch (Exception e)
        {
            var storeIds = context.Message.StorePrices.Select(x => x.ProductVariantId)
                .Distinct()
                .ToList();
            await ProduceError(_failureTopicProducer, context, storeIds);
            _logger.Error(e, "AddStorePriceRequestConsumer: {CorrelationId} - Error", context.Message.CorrelationId);
        }
    }
    private static async Task ProduceError(ITopicProducer<Null, AddStorePriceErrorModel> topicProducer,
        ConsumeContext<AddStorePriceRequestModel> context, List<Guid> storeIds)
    {
        var addStorePriceErrorModel = new AddStorePriceErrorModel()
        {
            CorrelationId = context.Message.CorrelationId,
            BrandId = context.Message.BrandId,
            BrandMenuId = context.Message.BrandMenuId,
            StoreIds = storeIds
        };
        await topicProducer.Produce(
            key: null,
            addStorePriceErrorModel,
            context.CancellationToken
        ).ConfigureAwait(false);
    }
}