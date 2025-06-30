using DimPos.Store.Application.Services.Interface;
using DimPos.Store.Domain.Entities;
using DimPos.Store.Domain.Models.Common;
using DimPos.Store.Domain.Models.Response;
using DimPos.Store.Infrastructure.Persistence;
using DimPos.Store.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.Store.Application.Features.FinancialShiftConfig.Query.GetFinancialShiftConfigs;

public class GetFinancialShiftConfigsQueryHandler : IRequestHandler<GetFinancialShiftConfigsQuery, ApiResponse>
{
    private readonly IUnitOfWork<StoreContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    
    public GetFinancialShiftConfigsQueryHandler(IUnitOfWork<StoreContext> unitOfWork, ILogger logger, IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(GetFinancialShiftConfigsQuery request, CancellationToken cancellationToken)
    {
        var storeId = _claimService.GetStoreId ?? Guid.Empty;
        if (storeId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy Id của cửa hàng");
        }

        var financialShiftConfigs = await _unitOfWork.GetRepository<FinancialShiftConfigs>().GetPagingListAsync(
            selector: x => new GetFinancialShiftConfigsResponse()
            {
                Id = x.Id,
                OpeningTime = x.OpeningTime,
                ClosingTime = x.ClosingTime,
                IsActive = x.IsActive,
                CreatedByAccountId = x.CreatedByAccountId,
                CreatedDate = x.CreatedDate,
                LastModifiedDate = x.LastModifiedDate
            },
            predicate: x => x.StoreId == storeId,
            page: request.Page,
            size: request.Size,
            sortBy: request.SortBy,
            isAsc: request.IsAsc
        );

        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Lấy danh sách cấu hình ca tài chính thành công",
            Data = financialShiftConfigs,
        };
    }
}