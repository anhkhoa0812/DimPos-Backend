using Confluent.Kafka;
using DimPos.Store.Application.Services.Interface;
using DimPos.Store.Domain.Enums;
using DimPos.Store.Domain.Models.Common;
using DimPos.Store.Infrastructure.Persistence;
using DimPos.Store.Infrastructure.Repositories.Interface;
using MassTransit;
using Mediator;
using Microsoft.EntityFrameworkCore;
using SharedProject.Events.Store.UpdateStoreByBrand;

namespace DimPos.Store.Application.Features.Stores.Command.UpdateStoreForBrand;

public class UpdateStoreForBrandCommandHandler : IRequestHandler<UpdateStoreForBrandCommand, ApiResponse>
{
    private readonly IUnitOfWork<StoreContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    private readonly ITopicProducer<Null, UpdateStoreByBrandRequestModel> _topicProducer;
    public UpdateStoreForBrandCommandHandler(IUnitOfWork<StoreContext> unitOfWork, ILogger logger, IClaimService claimService,
        ITopicProducer<Null, UpdateStoreByBrandRequestModel> topicProducer)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
        _topicProducer = topicProducer ?? throw new ArgumentNullException(nameof(topicProducer));
    }
    
    public async ValueTask<ApiResponse> Handle(UpdateStoreForBrandCommand request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if (brandId == Guid.Empty)
            throw new BadHttpRequestException("Không tìm thấy Id của thương hiệu");

        var store = await _unitOfWork.GetRepository<Domain.Entities.Store>().SingleOrDefaultAsync(
            predicate: x => x.Id == request.StoreId,
            include: x => x.Include(x => x.StoreAccounts)
        );
        if (store == null)
            throw new BadHttpRequestException("Không tìm thấy cửa hàng");
        
        store.StartingStoreCashLending = request.StartingStoreCashLending ?? store.StartingStoreCashLending;
        var updateStoreByBrandRequestModel = new UpdateStoreByBrandRequestModel()
        {
            StoreId = store.Id,
            CorrelationId = Guid.CreateVersion7()
        };
        if (request.Status != null && request.Status != store.Status)
        {
            if (request.Status == EStoreStatus.Active)
            {
                store.Status = EStoreStatus.Active;
                updateStoreByBrandRequestModel.AccountIds = store.StoreAccounts
                    .Where(x => x.Role == EStoreRole.StoreAdmin)
                    .Select(x => x.AccountId).Distinct().ToList();
                updateStoreByBrandRequestModel.Status = StoreStatus.Active;
            }
            else if (request.Status == EStoreStatus.Inactive)
            {
                store.Status = EStoreStatus.Inactive;
                updateStoreByBrandRequestModel.Status = StoreStatus.Inactive;
                updateStoreByBrandRequestModel.AccountIds = store.StoreAccounts
                    .Select(x => x.AccountId).Distinct().ToList();
            }
            else
            {
                throw new BadHttpRequestException("Trạng thái cửa hàng không hợp lệ");
            }
        }
        
        _unitOfWork.GetRepository<Domain.Entities.Store>().UpdateAsync(store);
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (!isSuccess)
        {
            throw new Exception("Cập nhật cửa hàng không thành công");
        }
        
        if( request.Status != null && request.Status != store.Status)
        {
            await _topicProducer.Produce(
                null,
                updateStoreByBrandRequestModel,
                cancellationToken: cancellationToken
            ).ConfigureAwait(false);
        }

        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Cập nhật cửa hàng thành công",
            Data = store.Id
        };
    }
}