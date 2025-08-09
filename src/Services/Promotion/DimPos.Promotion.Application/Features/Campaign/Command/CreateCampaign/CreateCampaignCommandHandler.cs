using DimPos.Promotion.Application.Services.Interface;
using DimPos.Promotion.Domain.Entities;
using DimPos.Promotion.Domain.Enums;
using DimPos.Promotion.Domain.Models.Common;
using DimPos.Promotion.Infrastructure.Persistence;
using DimPos.Promotion.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.Promotion.Application.Features.Campaign.Command.CreateCampaign;

public class CreateCampaignCommandHandler : IRequestHandler<CreateCampaignCommand, ApiResponse>
{
    private readonly IUnitOfWork<PromotionContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;

    public CreateCampaignCommandHandler(IUnitOfWork<PromotionContext> unitOfWork, ILogger logger, IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(CreateCampaignCommand request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        
        if(brandId == Guid.Empty)
            throw new BadHttpRequestException("Id của thương hiệu không hợp lệ.");
        
        var campaignId = Guid.CreateVersion7();
        var campaign = new Campaigns()
        {
            Id = campaignId,
            Name = request.Name,
            Description = request.Description,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Channel = ECampaignChannel.Pos,
            Priority = request.Priority,
            MaxTotalUsageLimit = request.MaxTotalUsageLimit,
            MaxUsagePerCustomerLimit = request.MaxUsagePerCustomerLimit,
            BrandId = brandId,
            IsActive = true
        };
        if (request.PromotionRuleIds != null && request.PromotionRuleIds.Any())
        {
            var promotionRules = await _unitOfWork.GetRepository<PromotionRules>().GetListAsync(
                predicate: x => request.PromotionRuleIds.Contains(x.Id) && 
                            x.BrandId == brandId && 
                            x.IsActive
            );
            
            if (promotionRules == null || !promotionRules.Any())
            {
                throw new BadHttpRequestException("Không tìm thấy quy tắc khuyến mãi hợp lệ.");
            }

            campaign.CampaignRuleLinks = promotionRules.Select(rule => new CampaignRuleLinks
            {
                Id = Guid.CreateVersion7(),
                CampaignId = campaignId,
                PromotionRuleId = rule.Id
            }).ToList();
        }
        await _unitOfWork.GetRepository<Campaigns>().InsertAsync(campaign);
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (!isSuccess)
        {
            _logger.Error("Tạo chiến dịch khuyến mãi thất bại");
            return new ApiResponse
            {
                Status = StatusCodes.Status500InternalServerError,
                Message = "Tạo chiến dịch khuyến mãi thất bại."
            };
        }
        _logger.Information("Tạo chiến dịch khuyến mãi thành công: {CampaignId}", campaignId);
        return new ApiResponse
        {
            Status = StatusCodes.Status201Created,
            Message = "Tạo chiến dịch khuyến mãi thành công.",
            Data = campaignId
        };
    }
}