using Confluent.Kafka;
using DimPos.Catalog.Domain.Entities;
using DimPos.Catalog.Infrastructure.Persistence;
using DimPos.Catalog.Infrastructure.Repositories.Interface;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using SharedProject.Events.RemoveMenuForStore;

namespace DimPos.Catalog.Application.Consumers;

public class RemoveStorePriceRequestConsumer : IConsumer<RemoveStorePriceRequestModel>
{
    private readonly IUnitOfWork<CatalogContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly ITopicProducer<Null, RemoveStorePriceResponseModel> _successTopicProducer;
    private readonly ITopicProducer<Null, RemoveStorePriceErrorModel> _failureTopicProducer;
    
    public RemoveStorePriceRequestConsumer(IUnitOfWork<CatalogContext> unitOfWork, ILogger logger,
        ITopicProducer<Null, RemoveStorePriceResponseModel> successTopicProducer,
        ITopicProducer<Null, RemoveStorePriceErrorModel> failureTopicProducer)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _successTopicProducer = successTopicProducer ?? throw new ArgumentNullException(nameof(successTopicProducer));
        _failureTopicProducer = failureTopicProducer ?? throw new ArgumentNullException(nameof(failureTopicProducer));
    }
    
    public async Task Consume(ConsumeContext<RemoveStorePriceRequestModel> context)
    {
        _logger.Information("RemoveStorePriceRequestConsumer: {CorrelationId}", context.Message.CorrelationId);
        try
        {
            var productVariantIds = context.Message.StorePrices
                .Select(x => x.ProductVariantId)
                .Distinct()
                .ToList();

            var storePrices = new List<StorePrice>();
            foreach (var storePriceRequest in context.Message.StorePrices)
            {
                var storePrice = await _unitOfWork.GetRepository<StorePrice>().GetListAsync(
                    predicate: x => x.StoreId == storePriceRequest.StoreId && 
                                    productVariantIds.Contains(x.ProductVariantId)
                );
                if (storePrice == null)
                    throw new BadHttpRequestException("Không tìm thấy giá của sản phẩm trong danh sách giá của cửa hàng trong lúc cập nhật cửa hàng áp dụng menu");
                storePrices.AddRange(storePrice);
            }

            _unitOfWork.GetRepository<StorePrice>().DeleteRangeAsync(storePrices);
            var isSuccess = await _unitOfWork.CommitAsync() > 0;
            if (isSuccess)
            {
                await _successTopicProducer.Produce(
                    null,
                    new RemoveStorePriceResponseModel()
                    {
                        CorrelationId = context.Message.CorrelationId
                    },
                    cancellationToken: context.CancellationToken
                ).ConfigureAwait(false);
                _logger.Information("RemoveStorePriceRequestConsumer: {CorrelationId} - Success", context.Message.CorrelationId);
            }
            else
            {
                _logger.Error("RemoveStorePriceRequestConsumer: {CorrelationId} - Failed", context.Message.CorrelationId);
                throw new Exception("Đã xảy ra lỗi khi xóa giá của sản phẩm trong danh sách giá của cửa hàng trong lúc cập nhật cửa hàng áp dụng menu");
            }
        }
        catch (Exception e)
        {
            var removeStorePriceErrorModel = new RemoveStorePriceErrorModel()
            {
                CorrelationId = context.Message.CorrelationId,
                BrandAccountId = context.Message.BrandAccountId,
                BrandId = context.Message.BrandId,
                StoreMenuAssignments = context.Message.StoreMenuAssignments,
                Message = e.Message
            };
            await _failureTopicProducer.Produce(
                key: null,
                removeStorePriceErrorModel,
                context.CancellationToken
            ).ConfigureAwait(false);
            _logger.Error(e, "RemoveStorePriceRequestConsumer: {CorrelationId} - Failed", context.Message.CorrelationId);
        }
    }
}