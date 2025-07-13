using DimPos.Store.Application.Services.Interface;
using DimPos.Store.Domain.Entities;
using DimPos.Store.Domain.Models.Common;
using DimPos.Store.Domain.Models.Response;
using DimPos.Store.Infrastructure.Persistence;
using DimPos.Store.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.Store.Application.Features.FinancialShift.Query.GetFinancialShiftById;

public class GetFinancialShiftByIdQueryHandler : IRequestHandler<GetFinancialShiftByIdQuery, ApiResponse>
{
    private readonly IUnitOfWork<StoreContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    
    public GetFinancialShiftByIdQueryHandler(IUnitOfWork<StoreContext> unitOfWork, ILogger logger, IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(GetFinancialShiftByIdQuery request, CancellationToken cancellationToken)
    {
        var storeId = _claimService.GetStoreId ?? Guid.Empty;
        if (storeId == Guid.Empty)
            throw new BadHttpRequestException("Không tìm thấy Id của cửa hàng");

        var financialShift = await _unitOfWork.GetRepository<FinancialShifts>().SingleOrDefaultAsync(
            predicate: x => x.Id == request.FinancialShiftId && x.FinancialShiftConfigs.StoreId == storeId
        );
        
        if (financialShift == null)
        {
            throw new BadHttpRequestException("Không tìm thấy ca tài chính");
        }
        
        var response = new GetFinancialShiftByIdResponse()
        {
            Id = financialShift.Id,
            OpeningTimestamp = financialShift.OpeningTimestamp,
            OpenedByAccountId = financialShift.OpenedByAccountId,
            OpeningCashExpected = financialShift.OpeningCashExpected,
            OpeningCashActual = financialShift.OpeningCashActual,
            OpeningDifferenceReason = financialShift.OpeningDifferenceReason,
            ClosingTimestamp = financialShift.ClosingTimestamp,
            ClosedByAccountId = financialShift.ClosedByAccountId,
            TotalGrossSalesInShift = financialShift.TotalGrossSalesInShift,
            TotalNetSalesInShift = financialShift.TotalNetSalesInShift,
            TotalTaxInShift = financialShift.TotalTaxInShift,
            TotalDiscountInShift = financialShift.TotalDiscountInShift,
            TotalCashRoundingInShift = financialShift.TotalCashRoundingInShift,
            Status = financialShift.Status,
            CreatedDate = financialShift.CreatedDate,
            LastModifiedDate = financialShift.LastModifiedDate
        };
        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Lấy thông tin ca tài chính thành công",
            Data = response
        };
    }
}