using Confluent.Kafka;
using DimPos.Catalog.Domain.Entities;
using DimPos.Catalog.Infrastructure.Persistence;
using DimPos.Catalog.Infrastructure.Repositories.Interface;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using SharedProject.Events.UpdateInventoryForInternalOrder;

namespace DimPos.Catalog.Application.Consumers;

public class GetIngredientDetailsRequestConsumer : IConsumer<GetIngredientDetailsRequestModel>
{
    private readonly IUnitOfWork<CatalogContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly ITopicProducer<Null, GetIngredientDetailsResponseModel> _successTopicProducer;
    private readonly ITopicProducer<Null, UpdateInventoryForInternalOrderErrorModel> _failureTopicProducer;
    
    public GetIngredientDetailsRequestConsumer(IUnitOfWork<CatalogContext> unitOfWork, ILogger logger,
        ITopicProducer<Null, GetIngredientDetailsResponseModel> successTopicProducer,
        ITopicProducer<Null, UpdateInventoryForInternalOrderErrorModel> failureTopicProducer)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _successTopicProducer = successTopicProducer ?? throw new ArgumentNullException(nameof(successTopicProducer));
        _failureTopicProducer = failureTopicProducer ?? throw new ArgumentNullException(nameof(failureTopicProducer));
    }
    
    public async Task Consume(ConsumeContext<GetIngredientDetailsRequestModel> context)
    {
        try
        {
            var productVariantIds = context.Message.StorePurchaseOrderItems.Select(x => x.ProductVariantId)
                .ToList();
            var productVariants = await _unitOfWork.GetRepository<ProductVariants>()
                .GetListAsync(
                    predicate: x => productVariantIds.Contains(x.Id),
                    include: x => x.Include(x => x.RecipeItems)
                        .ThenInclude(x => x.Ingredient)
                );
            if (productVariants.Count != productVariantIds.Count)
            {
                _logger.Error("Không tìm thấy thông tin của một số sản phẩm trong đơn hàng");
                throw new BadHttpRequestException($"Không tìm thấy thông tin của một số sản phẩm trong đơn hàng nội bộ {context.Message.StorePurchaseOrderId}");
            }

            var response = new GetIngredientDetailsResponseModel()
            {
                CorrelationId = context.Message.CorrelationId,
                AccountId = context.Message.AccountId,
                StoreId = context.Message.StoreId,
                StorePurchaseOrderId = context.Message.StorePurchaseOrderId,
            };
            foreach (var productVariant in productVariants)
            {
                var requestQuantity = context.Message.StorePurchaseOrderItems
                    .FirstOrDefault(x => x.ProductVariantId == productVariant.Id)?.ApprovedQuantityByBrand;
                if (requestQuantity == null)
                {
                    _logger.Error("Không tìm thấy số lượng yêu cầu cho sản phẩm {ProductVariantId}", productVariant.Id);
                    throw new BadHttpRequestException(
                        $"Không tìm thấy số lượng yêu cầu cho sản phẩm {productVariant.Id} trong đơn hàng nội bộ {context.Message.StorePurchaseOrderId}");
                }

                var ingredientDetailsModels = productVariant.RecipeItems!.Select(x => new IngredientDetailsModel()
                {
                    IngredientId = x.Ingredient.Id,
                    Quantity = x.Quantity * requestQuantity.Value,
                }).ToList();

                response.IngredientDetails.AddRange(ingredientDetailsModels);
            }

            _logger.Information("GetIngredientDetailsRequestConsumer: {CorrelationId}", context.Message.CorrelationId);

            await _successTopicProducer.Produce(
                key: null, 
                response, 
                cancellationToken: context.CancellationToken);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error while producing GetIngredientDetailsResponse");
            var errorResponse = new UpdateInventoryForInternalOrderErrorModel()
            {
                CorrelationId = context.Message.CorrelationId,
                AccountId = context.Message.
                StoreId = context.Message.StoreId,
                StorePurchaseOrderId = context.Message.StorePurchaseOrderId,
                Message = ex.Message
            };
            await _failureTopicProducer.Produce(
                key: null,
                errorResponse,
                cancellationToken: context.CancellationToken
            );
        }

    }
}