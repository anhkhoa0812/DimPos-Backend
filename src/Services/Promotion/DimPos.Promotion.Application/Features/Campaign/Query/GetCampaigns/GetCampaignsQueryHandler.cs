using DimPos.Promotion.Application.Services.Interface;
using DimPos.Promotion.Domain.Models.Campaigns;
using DimPos.Promotion.Domain.Models.Common;
using DimPos.Promotion.Infrastructure.Persistence;
using DimPos.Promotion.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.Promotion.Application.Features.Campaign.Query.GetCampaigns;

public class GetCampaignsQueryHandler : IRequestHandler<GetCampaignsQuery, ApiResponse>
{
    private readonly IUnitOfWork<PromotionContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    
    public GetCampaignsQueryHandler(IUnitOfWork<PromotionContext> unitOfWork, ILogger logger, IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(GetCampaignsQuery request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if (brandId == Guid.Empty)
            throw new BadHttpRequestException("Không tìm thấy Id thương hiệu");

        var campaigns = await _unitOfWork.GetRepository<Domain.Entities.Campaigns>().GetPagingListAsync(
            selector: x => new GetCampaignsResponse()
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                Status = x.Status,
                Channel = x.Channel,
                Priority = x.Priority,
                MaxTotalUsageLimit = x.MaxTotalUsageLimit,
                MaxUsagePerCustomerLimit = x.MaxUsagePerCustomerLimit
            },
            predicate: x => x.BrandId == brandId,
            page: request.Page,
            size: request.Size,
            sortBy: request.SortBy ?? "Priority",
            isAsc: request.IsAsc
        );
        return new ApiResponse
        {
            Status = 200,
            Message = "Lấy danh sách chiến dịch thành công",
            Data = campaigns
        };

    }
}