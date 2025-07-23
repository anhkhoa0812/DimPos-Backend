using DimPos.Identity.Application.Common.Protos;
using DimPos.Store.Application.Services.Interface;
using DimPos.Store.Domain.Entities;
using DimPos.Store.Domain.Models.Common;
using DimPos.Store.Domain.Models.Response;
using DimPos.Store.Infrastructure.Persistence;
using DimPos.Store.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.Store.Application.Features.FinancialShift.Query.GetFinancialShifts;

public class GetFinancialShiftsQueryHandler : IRequestHandler<GetFinancialShiftsQuery, ApiResponse>
{
    private readonly IUnitOfWork<StoreContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    private readonly IdentityGrpcService.IdentityGrpcServiceClient _identityGrpcService;
    public GetFinancialShiftsQueryHandler(IUnitOfWork<StoreContext> unitOfWork, ILogger logger, IClaimService claimService,
        IdentityGrpcService.IdentityGrpcServiceClient identityGrpcService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
        _identityGrpcService = identityGrpcService ?? throw new ArgumentNullException(nameof(identityGrpcService));
    }
    
    public async ValueTask<ApiResponse> Handle(GetFinancialShiftsQuery request, CancellationToken cancellationToken)
    {
        var storeId = _claimService.GetStoreId ?? Guid.Empty;
        if (storeId == Guid.Empty)
            throw new BadHttpRequestException("Không tìm thấy Id của cửa hàng");

        var financialShifts = await _unitOfWork.GetRepository<FinancialShifts>().GetPagingListAsync(
            selector: x => new GetFinancialShiftsResponse()
            {
                Id = x.Id,
                OpeningTimestamp = x.OpeningTimestamp,
                OpenedByAccountId = x.OpenedByAccountId,
                OpeningCashExpected = x.OpeningCashExpected,
                OpeningCashActual = x.OpeningCashActual,
                OpeningDifferenceReason = x.OpeningDifferenceReason,
                ClosingTimestamp = x.ClosingTimestamp,
                ClosedByAccountId = x.ClosedByAccountId,
                TotalGrossSalesInShift = x.TotalGrossSalesInShift,
                TotalNetSalesInShift = x.TotalNetSalesInShift,
                TotalTaxInShift = x.TotalTaxInShift,
                TotalDiscountInShift = x.TotalDiscountInShift,
                TotalCashRoundingInShift = x.TotalCashRoundingInShift,
                Status = x.Status,
                CreatedDate = x.CreatedDate,
                LastModifiedDate = x.LastModifiedDate
            },
            predicate: x => x.FinancialShiftConfigs.StoreId == storeId,
            page: request.Page,
            size: request.Size,
            sortBy: request.SortBy ?? "CreatedDate",
            isAsc: request.IsAsc
        );
        var accountIds = new List<string>();
        accountIds.AddRange(financialShifts.Items
            .Select(x => x.OpenedByAccountId.ToString()).ToList());
        if(financialShifts.Items.Select(x => x.ClosedByAccountId).Any())
        {
            foreach (var closedAccountId in financialShifts.Items.Select(x => x.ClosedByAccountId))
            {
                if (closedAccountId != null && closedAccountId != Guid.Empty && !accountIds.Contains(closedAccountId.Value.ToString()))
                {
                    accountIds.Add(closedAccountId.Value.ToString());
                }
            }
        }
        var accounts = await _identityGrpcService.GetStaffDetailAsync(new GetStaffDetailRequest()
        {
            AccountId = { accountIds }
        });
        foreach (var item in financialShifts.Items)
        {
            var openedByAccount = accounts.Staffs.FirstOrDefault(x => x.Id == item.OpenedByAccountId.ToString());
            if (openedByAccount != null)
            {
                item.OpenedByAccount = new StaffDetailsResponse()
                {
                    Id = Guid.Parse(openedByAccount.Id),
                    Code = openedByAccount.Code,
                    Username = openedByAccount.Username,
                    Email = openedByAccount.Email
                };
            }
            if (item.ClosedByAccountId != null && item.ClosedByAccountId != Guid.Empty)
            {
                var closedByAccount = accounts.Staffs.FirstOrDefault(x => x.Id == item.ClosedByAccountId.Value.ToString());
                if (closedByAccount != null)
                {
                    item.ClosedByAccount = new StaffDetailsResponse()
                    {
                        Id = Guid.Parse(closedByAccount.Id),
                        Code = closedByAccount.Code,
                        Username = closedByAccount.Username,
                        Email = closedByAccount.Email
                    };
                }
            }
        }
        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Lấy danh sách ca tài chính thành công",
            Data = financialShifts
        };
    }
}