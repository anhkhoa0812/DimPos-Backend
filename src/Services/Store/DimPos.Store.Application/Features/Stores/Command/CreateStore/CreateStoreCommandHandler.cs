using Confluent.Kafka;
using DimPos.Store.Application.Common.Mapper;
using DimPos.Store.Application.Common.Utils;
using DimPos.Store.Application.Services.Interface;
using DimPos.Store.Domain.Entities;
using DimPos.Store.Domain.Enums;
using DimPos.Store.Domain.Models.Common;
using DimPos.Store.Infrastructure.Persistence;
using DimPos.Store.Infrastructure.Repositories.Interface;
using MassTransit;
using Mediator;
using SharedProject.Events.Store.CreateStore;

namespace DimPos.Store.Application.Features.Stores.Command.CreateStore;

public class CreateStoreCommandHandler : IRequestHandler<CreateStoreCommand, ApiResponse>
{
    private readonly IUnitOfWork<StoreContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    private readonly ITopicProducer<Null, CreateStoreResponseModel> _producer;
    public CreateStoreCommandHandler(IUnitOfWork<StoreContext> unitOfWork, ILogger logger, IClaimService claimService,
        ITopicProducer<Null, CreateStoreResponseModel> producer)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
        _producer = producer ?? throw new ArgumentNullException(nameof(producer));
    }
    
    public async ValueTask<ApiResponse> Handle(CreateStoreCommand request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId;
        
        if (brandId.Equals(Guid.Empty))
        {
            return new ApiResponse()
            {
                Status = 401,
                Message = "Không tìm thấy thương hiệu",
                Data = null
            };
        }
        
        var store = StoreMapper.ToStores(request);
        store.Id = Guid.CreateVersion7();
        store.Status = EStoreStatus.Active;
        store.BrandId = brandId;
        await _unitOfWork.GetRepository<Domain.Entities.Store>().InsertAsync(store);
        var accountId = Guid.CreateVersion7();
        var storeAccount = new StoreAccounts()
        {
            Id = Guid.CreateVersion7(),
            StoreId = store.Id,
            AccountId = accountId,
            Role = EStoreRole.StoreAdmin
        };
        await _unitOfWork.GetRepository<StoreAccounts>().InsertAsync(storeAccount);
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (isSuccess)
        {
            var (passwordHash, passwordSalt) = PasswordUtil.HashPassword(request.Password);
            var createStoreResponseModel = new CreateStoreResponseModel()
            {
                StoreId = store.Id,
                AccountId = accountId,
                Code = store.Code,
                Email = store.Email,
                Username = request.Username,
                HashPassword = passwordHash,
                SaltPassword = passwordSalt,
                CorrelationId = Guid.CreateVersion7()
            };
            await _producer.Produce(
                key: null,
                createStoreResponseModel,
                cancellationToken: cancellationToken
            );
            _logger.Information($"Produce create store event to topic {nameof(CreateStoreResponseModel)}");
            return new ApiResponse()
            {
                Status = StatusCodes.Status201Created,
                Message = "Tạo mới cửa hàng thành công",
                Data = null
            };
        }
        return new ApiResponse()
        {
            Status = StatusCodes.Status500InternalServerError,
            Message = "Tạo mới cửa hàng thất bại",
            Data = null
        };
    }
}