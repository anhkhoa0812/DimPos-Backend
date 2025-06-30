using DimPos.Store.Application.Services.Interface;
using DimPos.Store.Domain.Entities;
using DimPos.Store.Domain.Models.Common;
using DimPos.Store.Domain.Models.Response;
using DimPos.Store.Infrastructure.Persistence;
using DimPos.Store.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.Store.Application.Features.FinancialShiftConfig.Query.GetFinancialShiftConfigById;

public class GetFinancialShiftConfigByIdQueryHandler : IRequestHandler<GetFinancialShiftConfigByIdQuery, ApiResponse>
{
    private readonly IUnitOfWork<StoreContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    
    public GetFinancialShiftConfigByIdQueryHandler(IUnitOfWork<StoreContext> unitOfWork, ILogger logger, IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(GetFinancialShiftConfigByIdQuery request, CancellationToken cancellationToken)
    {
        var storeId = _claimService.GetStoreId ?? Guid.Empty;
        if (storeId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy Id của cửa hàng");
        }

        var financialShiftConfig = await _unitOfWork.GetRepository<FinancialShiftConfigs>().SingleOrDefaultAsync(
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
            predicate: x => x.Id == request.FinancialShiftConfigId && x.StoreId == storeId
        );
        if (financialShiftConfig == null)
        {
            throw new BadHttpRequestException("Cấu hình ca tài chính không tồn tại");
        }
        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Lấy cấu hình ca tài chính thành công",
            Data = financialShiftConfig,
        };
    }
}