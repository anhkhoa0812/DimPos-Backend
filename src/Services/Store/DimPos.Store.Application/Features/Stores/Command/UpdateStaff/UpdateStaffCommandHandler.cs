using Confluent.Kafka;
using DimPos.Store.Application.Common.Utils;
using DimPos.Store.Application.Services.Interface;
using DimPos.Store.Domain.Models.Common;
using DimPos.Store.Infrastructure.Persistence;
using DimPos.Store.Infrastructure.Repositories.Interface;
using MassTransit;
using Mediator;
using SharedProject.Events.Store.UpdateStaff;

namespace DimPos.Store.Application.Features.Stores.Command.UpdateStaff;

public class UpdateStaffCommandHandler : IRequestHandler<UpdateStaffCommand, ApiResponse>
{
    private readonly IUnitOfWork<StoreContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    private readonly ITopicProducer<Null, UpdateStaffRequestModel> _topicProducer;
    public UpdateStaffCommandHandler(IUnitOfWork<StoreContext> unitOfWork, ILogger logger,
        IClaimService claimService, ITopicProducer<Null, UpdateStaffRequestModel> topicProducer)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
        _topicProducer = topicProducer ?? throw new ArgumentNullException(nameof(topicProducer));
    }
    
    public async ValueTask<ApiResponse> Handle(UpdateStaffCommand request, CancellationToken cancellationToken)
    {
        var storeId = _claimService.GetStoreId ?? Guid.Empty;
        if (storeId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy cửa hàng");
        }
        
        var storeAccount = await _unitOfWork.GetRepository<Domain.Entities.StoreAccounts>().SingleOrDefaultAsync(
            predicate: x => x.StoreId == storeId
            && x.AccountId == request.StaffId
            && x.Role == Domain.Enums.EStoreRole.Staff
        );
        
        if (storeAccount == null)
        {
            throw new BadHttpRequestException("Nhân viên không tồn tại trong cửa hàng này");
        }

        var model = new UpdateStaffRequestModel()
        {
            CorrelationId = Guid.CreateVersion7(),
            StaffId = storeAccount.AccountId,
            Code = request.Code,
            Email = request.Email,
            Status = (EAccountStatusForEvent?) request.Status,
            Username = request.Username,
        };
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

        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Cập nhật thông tin nhân viên thành công",
            Data = storeAccount.AccountId
        };
    }
}