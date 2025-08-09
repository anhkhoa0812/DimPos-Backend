using DimPos.Promotion.Application.Services.Interface;
using DimPos.Promotion.Domain.Entities;
using DimPos.Promotion.Domain.Models.Campaigns;
using DimPos.Promotion.Domain.Models.Common;
using DimPos.Promotion.Infrastructure.Persistence;
using DimPos.Promotion.Infrastructure.Repositories.Interface;
using DimPos.Store.Application.Common.Protos;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace DimPos.Promotion.Application.Features.Campaign.Query.GetCampaignById;

public class GetCampaignByIdQueryHandler : IRequestHandler<GetCampaignByIdQuery, ApiResponse>
{
    private readonly IUnitOfWork<PromotionContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    private readonly StoreGrpcService.StoreGrpcServiceClient _storeGrpcService;
    public GetCampaignByIdQueryHandler(IUnitOfWork<PromotionContext> unitOfWork, ILogger logger, IClaimService claimService,
        StoreGrpcService.StoreGrpcServiceClient storeGrpcService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
        _storeGrpcService = storeGrpcService ?? throw new ArgumentNullException(nameof(storeGrpcService));
    }
    
    public async ValueTask<ApiResponse> Handle(GetCampaignByIdQuery request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if(brandId == Guid.Empty)
            throw new BadHttpRequestException("Không tìm thấy Id thương hiệu");

        var campaign = await _unitOfWork.GetRepository<Campaigns>().SingleOrDefaultAsync(
            predicate: x => x.Id == request.CampaignId && x.BrandId == brandId,
            include: x => x.Include(x => x.CampaignStores)
                .Include(x => x.CampaignRuleLinks)
                .ThenInclude(x => x.PromotionRule)
        );
        if (campaign == null)
        {
            throw new BadHttpRequestException("Không tìm thấy chiến dịch khuyến mãi.");
        }

        var storeIds = campaign.CampaignStores?.Select(x => x.StoreId.ToString()).ToList();
        var storeResponse = new GetListStoreByStoreIdsResponse();
        if (storeIds != null && storeIds.Any())
        {
            storeResponse = await _storeGrpcService.GetListStoreByStoreIdsAsync(new GetListStoreByStoreIdsRequest()
            {
                StoreIds = { storeIds }
            });
        }
        var response = new GetCampaignByIdResponse()
        {
            Id = campaign.Id,
            Name = campaign.Name,
            Description = campaign.Description,
            StartDate = campaign.StartDate,
            EndDate = campaign.EndDate,
            Channel = campaign.Channel,
            Priority = campaign.Priority,
            MaxTotalUsageLimit = campaign.MaxTotalUsageLimit,
            MaxUsagePerCustomerLimit = campaign.MaxUsagePerCustomerLimit,
            IsActive = campaign.IsActive,
            PromotionRules = campaign.CampaignRuleLinks?.Select(pr => new PromotionRulesByGetCampaignByIdResponse()
            {
                Id = pr.PromotionRule.Id,
                Name = pr.PromotionRule.Name,
                Description = pr.PromotionRule.Description,
                ShortDescription = pr.PromotionRule.ShortDescription,
                IsActive = pr.PromotionRule.IsActive,
                Priority = pr.PromotionRule.Priority
            }).ToList(),
            Stores = storeResponse.Stores.Any() ? storeResponse.Stores.Select(store => new StoreByGetCampaignByIdResponse()
            {
                Id = Guid.Parse(store.Id),
                Code = store.Code,
                Name = store.Name,
                Phone = store.Phone,
                Email = store.Email,
                Description = store.Description,
                Address = store.Address,
                Latitude = store.Latitude,
                Longitude = store.Longitude
            }).ToList() : new List<StoreByGetCampaignByIdResponse>()
        };
        return new ApiResponse
        {
            Status = 200,
            Message = "Lấy thông tin chiến dịch thành công",
            Data = response
        };
        
    }
}