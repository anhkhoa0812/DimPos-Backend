using DimPos.Order.Application.Common.Protos;
using DimPos.Store.Application.Services.Interface;
using DimPos.Store.Domain.Entities;
using DimPos.Store.Domain.Enums;
using DimPos.Store.Domain.Models.Common;
using DimPos.Store.Infrastructure.Persistence;
using DimPos.Store.Infrastructure.Repositories.Interface;
using DimPos.Store.Infrastructure.Utils;
using Mediator;

namespace DimPos.Store.Application.Features.FinancialShift.Command.CloseFinancialShift;

public class CloseFinancialShiftCommandHandler : IRequestHandler<CloseFinancialShiftCommand, ApiResponse>
{
    private readonly IUnitOfWork<StoreContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    private readonly OrderGrpcService.OrderGrpcServiceClient _orderGrpcService;
    public CloseFinancialShiftCommandHandler(IUnitOfWork<StoreContext> unitOfWork, ILogger logger,
        IClaimService claimService,
        OrderGrpcService.OrderGrpcServiceClient orderGrpcService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
        _orderGrpcService = orderGrpcService ?? throw new ArgumentNullException(nameof(orderGrpcService));
    }
    
    public async ValueTask<ApiResponse> Handle(CloseFinancialShiftCommand request, CancellationToken cancellationToken)
    {
        var storeId = _claimService.GetStoreId ?? Guid.Empty;
        if (storeId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy thông tin cửa hàng");
        }

        var accountId = _claimService.GetCurrentUserId;
        if (accountId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy thông tin tài khoản");
        }

        var financialShift = await _unitOfWork.GetRepository<FinancialShifts>().SingleOrDefaultAsync(
            predicate: x => x.FinancialShiftConfigs.StoreId == storeId 
                            && x.Status == EFinancialShiftStatus.Open
        );
        if(financialShift == null)
            throw new BadHttpRequestException("Không tìm thấy ca tài chính đang mở cho cửa hàng này");
        
        financialShift.Status = EFinancialShiftStatus.Closed;
        financialShift.ClosedByAccountId = accountId;
        financialShift.ClosingTimestamp = TimeUtil.GetCurrentSEATime();

        var orderDetailResponse = await _orderGrpcService.GetDetailForCloseFinancialShiftAsync(
            new GetDetailForCloseFinancialShiftRequest()
            {
                FinancialShiftId = financialShift.Id.ToString(),
                StoreId = storeId.ToString()
            });
        
        financialShift.TotalGrossSalesInShift = (decimal) orderDetailResponse.TotalGrossSalesInShift;
        financialShift.TotalNetSalesInShift = (decimal)orderDetailResponse.TotalNetSalesInShift;
        financialShift.TotalTaxInShift = (decimal)orderDetailResponse.TotalTaxInShift;
        financialShift.TotalDiscountInShift = (decimal)orderDetailResponse.TotalDiscountInShift;
        financialShift.TotalCashRoundingInShift = (decimal)orderDetailResponse.TotalCashRoundingInShift;
        
        _unitOfWork.GetRepository<FinancialShifts>().UpdateAsync(financialShift);
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (!isSuccess)
        {
            throw new Exception("Đóng ca tài chính không thành công");
        }
        
        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Đóng ca tài chính thành công",
            Data = financialShift.Id
        };
    }
}