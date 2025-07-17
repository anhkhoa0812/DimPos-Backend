using Confluent.Kafka;
using DimPos.Store.Application.Common.Utils;
using DimPos.Store.Application.Services.Interface;
using DimPos.Store.Domain.Enums;
using DimPos.Store.Domain.Models.Common;
using DimPos.Store.Infrastructure.Persistence;
using DimPos.Store.Infrastructure.Repositories.Interface;
using MassTransit;
using Mediator;
using Microsoft.EntityFrameworkCore;
using SharedProject.Events.Store.UpdateStore;

namespace DimPos.Store.Application.Features.Stores.Command.UpdateStore;

public class UpdateStoreCommandHandler : IRequestHandler<UpdateStoreCommand, ApiResponse>
{
    private readonly IUnitOfWork<StoreContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    private readonly ITopicProducer<Null, UpdateStoreRequestModel> _topicProducer;

    public UpdateStoreCommandHandler(IUnitOfWork<StoreContext> unitOfWork, ILogger logger, IClaimService claimService,
        ITopicProducer<Null, UpdateStoreRequestModel> topicProducer)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
        _topicProducer = topicProducer ?? throw new ArgumentNullException(nameof(topicProducer));
    }
    
    public async ValueTask<ApiResponse> Handle(UpdateStoreCommand request, CancellationToken cancellationToken)
    {
        var storeId = _claimService.GetStoreId ?? Guid.Empty;
        if (storeId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy cửa hàng");
        }

        var store = await _unitOfWork.GetRepository<Domain.Entities.Store>().SingleOrDefaultAsync(
            predicate: x => x.Id == storeId,
            include: x => x.Include(x => x.StoreAccounts)
        );
        if (store == null)
        {
            throw new BadHttpRequestException("Cửa hàng không tồn tại");
        }
        
        store.Name = request.Name ?? store.Name;
        store.Code = request.Code ?? store.Code;
        store.Email = request.Email ?? store.Email;
        store.Phone = request.Phone ?? store.Phone;
        store.ShortName = request.ShortName ?? store.ShortName;
        store.Description = request.Description ?? store.Description;
        store.Address = request.Address ?? store.Address;
        store.Latitude = request.Latitude ?? store.Latitude;
        store.Longitude = request.Longitude ?? store.Longitude;
        store.WifiName = request.WifiName ?? store.WifiName;
        store.WifiPassword = request.WifiPassword ?? store.WifiPassword;
        store.Index = request.Index ?? store.Index;
        store.LocalPasscode = request.LocalPasscode ?? store.LocalPasscode;
        store.ManagerName = request.ManagerName ?? store.ManagerName;
        store.Type = request.Type ?? store.Type;
        store.StartingStoreCashLending = request.StartingStoreCashLending ?? store.StartingStoreCashLending;
        
        _unitOfWork.GetRepository<Domain.Entities.Store>().UpdateAsync(store);
        
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (!isSuccess)
        {
            throw new Exception("Cập nhật cửa hàng thất bại");
        }

        if (request.Username != null || request.Password != null)
        {
            var model = new UpdateStoreRequestModel();
            model.CorrelationId = Guid.CreateVersion7();
            model.AccountId = store.StoreAccounts.First(x => x.Role == EStoreRole.StoreAdmin).AccountId;
            model.Username = request.Username;
            if (request.Password != null)
            {
                var (passwordHash, passwordSalt) = PasswordUtil.HashPassword(request.Password);
                model.HashPassword = passwordHash;
                model.SaltPassword = passwordSalt;
            }
            await _topicProducer.Produce(
                key: null,
                model,
                cancellationToken: cancellationToken
            ).ConfigureAwait(false);
        }
        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Cập nhật thông tin cửa hàng thành công",
            Data = store.Id
        };
    }
}