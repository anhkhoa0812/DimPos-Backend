using DimPos.Catalog.Domain.Entities;
using DimPos.Catalog.Infrastructure.Persistence;
using DimPos.Catalog.Infrastructure.Repositories.Interface;
using DimPos.Catalog.Infrastructure.Utils;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using SharedProject.Events.UpdateBrandMenuItem;

namespace DimPos.Catalog.Application.Consumers;

public class CreateStorePriceForBrandMenuItemRequestConsumer : IConsumer<CreateStorePriceForBrandMenuItemRequestModel>
{
    private readonly IUnitOfWork<CatalogContext> _unitOfWork;
    private readonly ILogger _logger;
    
    public CreateStorePriceForBrandMenuItemRequestConsumer(IUnitOfWork<CatalogContext> unitOfWork, ILogger logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    public async Task Consume(ConsumeContext<CreateStorePriceForBrandMenuItemRequestModel> context)
    {
        if (context.Message.NewProductVariantIds != null && context.Message.NewProductVariantIds.Any())
        {
            var basePrices = await _unitOfWork.GetRepository<BasePrice>().GetListAsync(
                predicate: x => x.BrandId == context.Message.BrandId 
                            && context.Message.NewProductVariantIds.Contains(x.ProductVariantId)
            );
            foreach (var basePrice in basePrices)
            {
                foreach (var storeId in context.Message.StoreIds)
                {
                    var newStorePrice = new StorePrice()
                    {
                        Id = Guid.CreateVersion7(),
                        StoreId = storeId,
                        ProductVariantId = basePrice.ProductVariantId,
                        CurrencyCode = "VND",
                        OverridePrice = basePrice.Price,
                        EffectiveFrom = TimeUtil.GetCurrentSEATime(),
                        StorePriceHistories = new List<StorePriceHistory>()
                        {
                            new()
                            {
                                Id = Guid.CreateVersion7(),
                                CurrencyCode = "VND",
                                ProductVariantId = basePrice.ProductVariantId,
                                ChangedAt = TimeUtil.GetCurrentSEATime(),
                                StoreId = storeId,
                                OldPrice = 0,
                                NewPrice = basePrice.Price,
                            }
                        }
                    };
                    await _unitOfWork.GetRepository<StorePrice>().InsertAsync(newStorePrice);
                }
            }
        }

        if (context.Message.RemovedProductVariantIds != null && context.Message.RemovedProductVariantIds.Any())
        {
            foreach (var removedProductVariantId in context.Message.RemovedProductVariantIds)
            {
                var storePrices = await _unitOfWork.GetRepository<StorePrice>().GetListAsync(
                    predicate: x => x.ProductVariantId == removedProductVariantId &&
                                    context.Message.StoreIds.Contains(x.StoreId),
                    include: x => x.Include(x => x.StorePriceHistories)
                );
                if (storePrices.Any())
                {
                    var historiesToDelete = storePrices.Select(x => x.StorePriceHistories).SelectMany(x => x).ToList();
                    _unitOfWork.GetRepository<StorePriceHistory>().DeleteRangeAsync(historiesToDelete);
                }
                _unitOfWork.GetRepository<StorePrice>().DeleteRangeAsync(storePrices);
            }
        }

        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (isSuccess)
        {
            _logger.Information("Created store prices for brand menu items successfully. CorrelationId: {CorrelationId}",
                context.Message.CorrelationId);
        }
        else
        {
            _logger.Error("Failed to create store prices for brand menu items.");
        }
    }
}