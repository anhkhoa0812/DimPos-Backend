using DimPos.Store.Application.Services.Interface;
using DimPos.Store.Domain.Entities;
using DimPos.Store.Domain.Models.Common;
using DimPos.Store.Infrastructure.Persistence;
using DimPos.Store.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.Store.Application.Features.FinancialShiftConfig.Command.CreateFinancialShiftConfig;

public class CreateFinancialShiftConfigCommandHandler : IRequestHandler<CreateFinancialShiftConfigCommand, ApiResponse>
{
    private readonly IUnitOfWork<StoreContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    
    public CreateFinancialShiftConfigCommandHandler(IUnitOfWork<StoreContext> unitOfWork, ILogger logger, IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(CreateFinancialShiftConfigCommand request, CancellationToken cancellationToken)
    {
        var storeId = _claimService.GetStoreId ?? Guid.Empty;
        if (storeId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy Id của cửa hàng");
        }

        var store = await _unitOfWork.GetRepository<Domain.Entities.Store>().SingleOrDefaultAsync(
            predicate: x => x.Id == storeId
        );
        if (store == null)
        {
            throw new BadHttpRequestException("Cửa hàng không tồn tại");
        }
        
        var accountId = _claimService.GetCurrentUserId;
        if (accountId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy Id của tài khoản");
        }
        
        var financialShiftConfig = new FinancialShiftConfigs()
        {
            Id = Guid.CreateVersion7(),
            StoreId = store.Id,
            OpeningTime = request.OpeningTime,
            ClosingTime = request.ClosingTime,
            CreatedByAccountId = accountId,
            IsActive = false,
        };
        await _unitOfWork.GetRepository<FinancialShiftConfigs>().InsertAsync(financialShiftConfig);
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (!isSuccess)
        {
            throw new Exception("Tạo cấu hình ca tài chính không thành công");
        }
        _logger.Information("Tạo cấu hình ca tài chính thành công cho cửa hàng {StoreId}", storeId);
        return new ApiResponse
        {
            Status = StatusCodes.Status201Created,
            Message = "Tạo cấu hình ca tài chính thành công",
            Data = null
        };
    }
}