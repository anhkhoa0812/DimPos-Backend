using DimPos.Store.Application.Services.Interface;
using DimPos.Store.Domain.Entities;
using DimPos.Store.Domain.Enums;
using DimPos.Store.Domain.Models.Common;
using DimPos.Store.Infrastructure.Persistence;
using DimPos.Store.Infrastructure.Repositories.Interface;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace DimPos.Store.Application.Features.FinancialShift.Command.OpenFinancialShift;

public class OpenFinancialShiftCommandHandler : IRequestHandler<OpenFinancialShiftCommand, ApiResponse>
{
    private readonly IUnitOfWork<StoreContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;

    public OpenFinancialShiftCommandHandler(IUnitOfWork<StoreContext> unitOfWork, ILogger logger,
        IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(OpenFinancialShiftCommand request, CancellationToken cancellationToken)
    {
        var staffAccountId = _claimService.GetCurrentUserId;
        if (staffAccountId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy thông tin tài khoản nhân viên");
        }

        var storeId = _claimService.GetStoreId ?? Guid.Empty;
        if (storeId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy thông tin cửa hàng");
        }
        
        var financialShiftConfigList = await _unitOfWork.GetRepository<FinancialShiftConfigs>().GetListAsync(
            predicate: x => x.StoreId.Equals(storeId),
            include: x => x.Include(x => x.FinancialShifts)
                .Include(x => x.Store)
        );
        if(financialShiftConfigList == null || !financialShiftConfigList.Any())
        {
            throw new BadHttpRequestException("Không tìm thấy cấu hình ca tài chính cho cửa hàng này");
        }
        var now = TimeOnly.FromDateTime(DateTime.UtcNow);
        var targetFinancialShiftConfig = financialShiftConfigList.FirstOrDefault(
            x => x.OpeningTime <= now && 
                 x.ClosingTime >= now && 
                 x.IsActive
        );
        
        if (targetFinancialShiftConfig == null)
        {
            throw new BadHttpRequestException("Không tìm thấy cấu hình ca tài chính phù hợp với thời gian hiện tại");
        }
        
        if(financialShiftConfigList.Count(x => x.FinancialShifts != null && 
                                               x.FinancialShifts.Any(x => x.Status == EFinancialShiftStatus.Open)) > 0)
        {
            throw new BadHttpRequestException("Cửa hàng này đã có ca tài chính đang mở");
        }
        if(request.OpeningCashActual != targetFinancialShiftConfig.Store.StartingStoreCashLending)
        {
            if(string.IsNullOrWhiteSpace(request.OpeningDifferenceReason))
            {
                throw new BadHttpRequestException("Lý do chênh lệch không được để trống khi số tiền mở ca không khớp với số tiền dự kiến");
            }
        }

        var financialShift = new FinancialShifts()
        {
            Id = Guid.CreateVersion7(),
            OpeningTimestamp = DateTime.UtcNow,
            OpenedByAccountId = staffAccountId,
            OpeningCashExpected = targetFinancialShiftConfig.Store.StartingStoreCashLending,
            OpeningCashActual = request.OpeningCashActual,
            OpeningDifferenceReason = request.OpeningDifferenceReason,
            Status = EFinancialShiftStatus.Open,
            FinancialShiftConfigId = targetFinancialShiftConfig.Id,
        };
        await _unitOfWork.GetRepository<FinancialShifts>().InsertAsync(financialShift);
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (!isSuccess)
        {
            throw new Exception("Mở ca tài chính không thành công");
        }
        _logger.Information("Mở ca tài chính thành công cho cửa hàng {StoreId}", storeId);
        return new ApiResponse
        {
            Status = StatusCodes.Status201Created,
            Message = "Mở ca tài chính thành công",
            Data = null
        };
    }
}