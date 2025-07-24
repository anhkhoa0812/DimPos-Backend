using Confluent.Kafka;
using DimPos.Identity.Domain.Entities;
using DimPos.Identity.Domain.Enum;
using DimPos.Identity.Infrastructure.Persistence;
using DimPos.Identity.Infrastructure.Repositories.Interface;
using MassTransit;
using SharedProject.Events.Store.UpdateStoreByBrand;

namespace DimPos.Identity.Application.Consumers;

public class UpdateAccountForStoreByBrandRequestConsumer : IConsumer<UpdateAccountForStoreByBrandRequestModel>
{
    private readonly IUnitOfWork<IdentityContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly ITopicProducer<Null, UpdateAccountForStoreByBrandResponseModel> _successTopicProducer;
    private readonly ITopicProducer<Null, UpdateAccountForStoreByBrandErrorModel> _errorTopicProducer;
    
    public UpdateAccountForStoreByBrandRequestConsumer(IUnitOfWork<IdentityContext> unitOfWork, ILogger logger,
        ITopicProducer<Null, UpdateAccountForStoreByBrandResponseModel> successTopicProducer,
        ITopicProducer<Null, UpdateAccountForStoreByBrandErrorModel> errorTopicProducer)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _successTopicProducer = successTopicProducer ?? throw new ArgumentNullException(nameof(successTopicProducer));
        _errorTopicProducer = errorTopicProducer ?? throw new ArgumentNullException(nameof(errorTopicProducer));
    }
    
    public async Task Consume(ConsumeContext<UpdateAccountForStoreByBrandRequestModel> context)
    {
        try
        {
            var accounts = await _unitOfWork.GetRepository<Accounts>().GetListAsync(
                predicate: x => context.Message.AccountIds.Contains(x.Id)
            );
            if(accounts.Count != context.Message.AccountIds.Count)
            {
                _logger.Warning("Some accounts not found for store update: {AccountIds}", context.Message.AccountIds);
                throw new BadHttpRequestException("Một số tài khoản không được tìm thấy khi cập nhật cửa hàng");
            }
            foreach (var account in accounts)
            {
                if (context.Message.Status == StoreStatus.Active)
                {
                    account.Status = EAccountStatus.Active;
                }
                else if (context.Message.Status == StoreStatus.Inactive)
                {
                    account.Status = EAccountStatus.Inactive;
                }
            }
            _unitOfWork.GetRepository<Accounts>().UpdateRange(accounts);
            var isSuccess = await _unitOfWork.CommitAsync() > 0;
            if (!isSuccess)
            {
                _logger.Error("Failed to update accounts for store: {StoreId}", context.Message.StoreId);
                throw new Exception("Cập nhật tài khoản cho cửa hàng không thành công");
            }
            await _successTopicProducer.Produce(
                key: null,
                new UpdateAccountForStoreByBrandResponseModel
                {
                    CorrelationId = context.Message.CorrelationId,
                    StoreId = context.Message.StoreId,
                    Status = context.Message.Status
                },
                context.CancellationToken);
            _logger.Information("Successfully updated accounts for store: {StoreId}", context.Message.StoreId);
            
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error updating accounts for store: {StoreId}", context.Message.StoreId);
            await _errorTopicProducer.Produce(
                key: null,
                new UpdateAccountForStoreByBrandErrorModel
                {
                    CorrelationId = context.Message.CorrelationId,
                    StoreId = context.Message.StoreId,
                    Status = context.Message.Status
                },
                context.CancellationToken);
        }
    }
}