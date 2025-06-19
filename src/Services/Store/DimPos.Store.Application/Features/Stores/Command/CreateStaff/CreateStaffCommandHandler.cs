using Confluent.Kafka;
using DimPos.Store.Application.Common.Utils;
using DimPos.Store.Application.Features.Stores.Command.CreateStore;
using DimPos.Store.Application.Services.Interface;
using DimPos.Store.Domain.Entities;
using DimPos.Store.Domain.Enums;
using DimPos.Store.Domain.Models.Common;
using DimPos.Store.Infrastructure.Persistence;
using DimPos.Store.Infrastructure.Repositories.Interface;
using MassTransit;
using Mediator;
using SharedProject.Events.Store.CreateStaff;
using SharedProject.Events.Store.CreateStore;

namespace DimPos.Store.Application.Features.Stores.Command.CreateStaff;

public class CreateStaffCommandHandler : IRequestHandler<CreateStaffCommand, ApiResponse>
{
    private readonly IUnitOfWork<StoreContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    private readonly ITopicProducer<Null, CreateStaffResponseModel> _producer;
    public CreateStaffCommandHandler(IUnitOfWork<StoreContext> unitOfWork, ILogger logger,
        IClaimService claimService, ITopicProducer<Null, CreateStaffResponseModel> producer)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
        _producer = producer ?? throw new ArgumentNullException(nameof(producer));
    }
    
    public async ValueTask<ApiResponse> Handle(CreateStaffCommand request, CancellationToken cancellationToken)
    {
        var storeId = _claimService.GetStoreId;
        if (storeId == null || storeId == Guid.Empty)
            throw new BadHttpRequestException("Không tìm thấy ID của cửa hàng");

        var store = await _unitOfWork.GetRepository<Domain.Entities.Store>().SingleOrDefaultAsync(
            predicate: x => x.Status == EStoreStatus.Active && x.Id == storeId
        );
        if (store == null)
            throw new BadHttpRequestException("Không tìm thấy cửa hàng");
        var accountId = Guid.CreateVersion7();
        var storeAccount = new StoreAccounts()
        {
            Id = Guid.CreateVersion7(),
            StoreId = store.Id,
            Role = EStoreRole.Staff,
            AccountId = accountId,
            AssignAt = DateTime.UtcNow
        };
        await _unitOfWork.GetRepository<StoreAccounts>().InsertAsync(storeAccount);

        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (!isSuccess)
            throw new Exception("Một lỗi đã xảy ra khi tạo Staff");
        var (passwordHash, passwordSalt) = PasswordUtil.HashPassword(request.Password);
        var createStaffResponseModel = new CreateStaffResponseModel()
        {
            StoreId = store.Id,
            AccountId = accountId,
            Code = request.Code,
            Email = request.Email,
            Username = request.Username,
            HashPassword = passwordHash,
            SaltPassword = passwordSalt
        };
        await _producer.Produce(
            key: null,
            createStaffResponseModel,
            cancellationToken: cancellationToken
        ).ConfigureAwait(false);
        _logger.Information($"Produce create staff event to topic {nameof(CreateStaffResponseModel)}");
        return new ApiResponse()
        {
            Status = StatusCodes.Status201Created,
            Message = "Tạo Staff thành công",
            Data = null
        };
    }
}