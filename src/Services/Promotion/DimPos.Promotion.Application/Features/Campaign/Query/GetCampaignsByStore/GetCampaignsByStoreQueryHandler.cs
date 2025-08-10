using DimPos.Promotion.Application.Services.Interface;
using DimPos.Promotion.Domain.Entities;
using DimPos.Promotion.Domain.Models.Campaigns;
using DimPos.Promotion.Domain.Models.Common;
using DimPos.Promotion.Infrastructure.Persistence;
using DimPos.Promotion.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.Promotion.Application.Features.Campaign.Query.GetCampaignsByStore;

public class GetCampaignsByStoreQueryHandler : IRequestHandler<GetCampaignsByStoreQuery, ApiResponse>
{
    private readonly IUnitOfWork<PromotionContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    
    public GetCampaignsByStoreQueryHandler(IUnitOfWork<PromotionContext> unitOfWork, ILogger logger, IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(GetCampaignsByStoreQuery request, CancellationToken cancellationToken)
    {
        var storeId = _claimService.GetStoreId ?? Guid.Empty;
        if (storeId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy Id cửa hàng");
        }

        var campaigns = await _unitOfWork.GetRepository<Campaigns>().GetPagingListAsync(
            selector: x => new GetCampaignsResponse()
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                IsActive = x.IsActive,
                Channel = x.Channel,
                Priority = x.Priority,
                MaxTotalUsageLimit = x.MaxTotalUsageLimit,
                MaxUsagePerCustomerLimit = x.MaxUsagePerCustomerLimit
            },
            predicate: x => x.CampaignStores != null && x.CampaignStores.Any(x => x.StoreId == storeId),
            page: request.Page,
            size: request.Size,
            sortBy: request.SortBy,
            isAsc: request.IsAsc
        );
        return new ApiResponse
        {
            Status = 200,
            Message = "Lấy danh sách chiến dịch theo cửa hàng thành công",
            Data = campaigns
        };
    }
}