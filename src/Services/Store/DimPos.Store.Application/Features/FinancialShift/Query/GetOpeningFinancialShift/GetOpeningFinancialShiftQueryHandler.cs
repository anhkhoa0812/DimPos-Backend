using DimPos.Store.Application.Services.Interface;
using DimPos.Store.Domain.Entities;
using DimPos.Store.Domain.Enums;
using DimPos.Store.Domain.Models.Common;
using DimPos.Store.Domain.Models.Response;
using DimPos.Store.Infrastructure.Persistence;
using DimPos.Store.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.Store.Application.Features.FinancialShift.Query.GetOpeningFinancialShift;

public class GetOpeningFinancialShiftQueryHandler : IRequestHandler<GetOpeningFinancialShiftQuery, ApiResponse>
{
    private readonly IUnitOfWork<StoreContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    
    public GetOpeningFinancialShiftQueryHandler(IUnitOfWork<StoreContext> unitOfWork, ILogger logger, IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(GetOpeningFinancialShiftQuery request, CancellationToken cancellationToken)
    {
        var storeId = _claimService.GetStoreId ?? Guid.Empty;
        if (storeId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy ID của cửa hàng");
        }

        var openingFinancialShift = await _unitOfWork.GetRepository<FinancialShifts>().SingleOrDefaultAsync(
            predicate: x => x.FinancialShiftConfigs.StoreId == storeId && x.Status == EFinancialShiftStatus.Open
        );
        
        
        var response = openingFinancialShift != null 
            ? new GetOpeningFinancialShiftResponse()
            {
                Id = openingFinancialShift.Id,
                OpeningTimestamp = openingFinancialShift.OpeningTimestamp,
                OpenedByAccountId = openingFinancialShift.OpenedByAccountId,
                OpeningCashExpected = openingFinancialShift.OpeningCashExpected,
                OpeningCashActual = openingFinancialShift.OpeningCashActual,
                OpeningDifferenceReason = openingFinancialShift.OpeningDifferenceReason,
                ClosingTimestamp = openingFinancialShift.ClosingTimestamp,
                ClosedByAccountId = openingFinancialShift.ClosedByAccountId,
                TotalGrossSalesInShift = openingFinancialShift.TotalGrossSalesInShift,
                TotalNetSalesInShift = openingFinancialShift.TotalNetSalesInShift,
                TotalTaxInShift = openingFinancialShift.TotalTaxInShift,
                TotalDiscountInShift = openingFinancialShift.TotalDiscountInShift,
                TotalCashRoundingInShift = openingFinancialShift.TotalCashRoundingInShift,
                Status = openingFinancialShift.Status,
                CreatedDate = openingFinancialShift.CreatedDate,
                LastModifiedDate = openingFinancialShift.LastModifiedDate
            }
            : null;

        return new ApiResponse()
        {
            Status = 200,
            Message = "Lấy ca tài chính mở thành công",
            Data = response
        };
    }
}