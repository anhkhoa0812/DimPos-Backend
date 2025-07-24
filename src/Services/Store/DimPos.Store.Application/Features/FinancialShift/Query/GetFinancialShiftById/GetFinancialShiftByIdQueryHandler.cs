using DimPos.Identity.Application.Common.Protos;
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
    private readonly IdentityGrpcService.IdentityGrpcServiceClient _identityGrpcService;
    public GetFinancialShiftByIdQueryHandler(IUnitOfWork<StoreContext> unitOfWork, ILogger logger, IClaimService claimService, 
        IdentityGrpcService.IdentityGrpcServiceClient identityGrpcService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
        _identityGrpcService = identityGrpcService ?? throw new ArgumentNullException(nameof(identityGrpcService));
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
        
        var accountIds = new List<string>();
        accountIds.Add(financialShift.OpenedByAccountId.ToString());
        if(financialShift.ClosedByAccountId != null && financialShift.ClosedByAccountId != financialShift.OpenedByAccountId)
        {
            accountIds.Add(financialShift.ClosedByAccountId.Value.ToString());
        }
        var accounts = await _identityGrpcService.GetStaffDetailAsync(new GetStaffDetailRequest()
        {
            AccountId = { accountIds }
        });
        
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
            LastModifiedDate = financialShift.LastModifiedDate,
        };
        var openedByAccount = accounts.Staffs.FirstOrDefault(x => x.Id == financialShift.OpenedByAccountId.ToString());
        if (openedByAccount != null)
        {
            response.OpenedByAccount = new StaffDetailsResponse()
            {
                Id = Guid.Parse(openedByAccount.Id),
                Username = openedByAccount.Username,
                Email = openedByAccount.Email,
                Code = openedByAccount.Code
            };
        }
        if (financialShift.ClosedByAccountId != null)
        {
            var closedByAccount = accounts.Staffs.FirstOrDefault(x => x.Id == financialShift.ClosedByAccountId.Value.ToString());
            if (closedByAccount != null)
            {
                response.ClosedByAccount = new StaffDetailsResponse()
                {
                    Id = Guid.Parse(closedByAccount.Id),
                    Username = closedByAccount.Username,
                    Email = closedByAccount.Email,
                    Code = closedByAccount.Code
                };
            }
        }
        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Lấy thông tin ca tài chính thành công",
            Data = response
        };
    }
}