using DimPos.Store.Application.Services.Interface;
using DimPos.Store.Domain.Entities;
using DimPos.Store.Domain.Enums;
using DimPos.Store.Domain.Models.Common;
using DimPos.Store.Infrastructure.Persistence;
using DimPos.Store.Infrastructure.Repositories.Interface;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace DimPos.Store.Application.Features.FinancialShiftConfig.Command.UpdateFinancialShiftConfig;

public class UpdateFinancialShiftConfigCommandHandler : IRequestHandler<UpdateFinancialShiftConfigCommand, ApiResponse>
{
    private readonly IUnitOfWork<StoreContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    
    public UpdateFinancialShiftConfigCommandHandler(IUnitOfWork<StoreContext> unitOfWork, ILogger logger, IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(UpdateFinancialShiftConfigCommand request, CancellationToken cancellationToken)
    {
        var storeId = _claimService.GetStoreId ?? Guid.Empty;
        if (storeId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy Id của cửa hàng");
        }

        var financialShiftConfigList = await _unitOfWork.GetRepository<FinancialShiftConfigs>().GetListAsync(
            predicate: x => x.StoreId == storeId,
            include: x => x.Include(x => x.FinancialShifts)
        );
        if (financialShiftConfigList == null || !financialShiftConfigList.Any())
        {
            throw new BadHttpRequestException("Không tìm thấy cấu hình ca tài chính cho cửa hàng này");
        }
        var financialShiftConfig = financialShiftConfigList.FirstOrDefault(x => x.Id == request.FinancialShiftConfigId);
        
        if (financialShiftConfig == null)
        {
            throw new BadHttpRequestException("Cấu hình ca tài chính không tồn tại hoặc không thuộc cửa hàng này");
        }
        
        if(financialShiftConfig.FinancialShifts != null &&
           financialShiftConfig.FinancialShifts.Any(x => x.Status == EFinancialShiftStatus.Open))
        {
            throw new BadHttpRequestException("Không thể cập nhật cấu hình ca tài chính khi có ca tài chính đang mở");
        }
        financialShiftConfig.OpeningTime = request.OpeningTime;
        financialShiftConfig.ClosingTime = request.ClosingTime;
        if (!request.IsActive)
        {
            if (!financialShiftConfigList.Where(x => x != financialShiftConfig)
                    .Any(x => x.IsActive))
            {
                throw new BadHttpRequestException("Không thể vô hiệu hóa cấu hình ca tài chính khi không còn cấu hình nào khác đang hoạt động");
            }
        }
        else
        {
            foreach (var financialShiftConfigItem in financialShiftConfigList)
            {
                financialShiftConfigItem.IsActive = false;
            }
        }
        financialShiftConfig.IsActive = request.IsActive;
        
        _unitOfWork.GetRepository<FinancialShiftConfigs>().UpdateRange(financialShiftConfigList);
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (!isSuccess)
        {
            throw new Exception("Cập nhật cấu hình ca tài chính không thành công");
        }
        
        _logger.Information("Cập nhật cấu hình ca tài chính thành công cho cửa hàng {StoreId}", storeId);
        return new ApiResponse
        {
            Status = StatusCodes.Status200OK,
            Message = "Cập nhật cấu hình ca tài chính thành công",
            Data = null
        };
        
    }
}