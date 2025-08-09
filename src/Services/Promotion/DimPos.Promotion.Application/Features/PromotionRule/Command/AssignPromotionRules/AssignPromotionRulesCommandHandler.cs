using DimPos.Promotion.Application.Services.Interface;
using DimPos.Promotion.Domain.Entities;
using DimPos.Promotion.Domain.Models.Common;
using DimPos.Promotion.Infrastructure.Persistence;
using DimPos.Promotion.Infrastructure.Repositories.Interface;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace DimPos.Promotion.Application.Features.PromotionRule.Command.AssignPromotionRules;

public class AssignPromotionRulesCommandHandler : IRequestHandler<AssignPromotionRulesCommand, ApiResponse>
{
    private readonly IUnitOfWork<PromotionContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    
    public AssignPromotionRulesCommandHandler(IUnitOfWork<PromotionContext> unitOfWork, ILogger logger, IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(AssignPromotionRulesCommand request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if (brandId == Guid.Empty)
            throw new BadHttpRequestException("Không tìm thấy Id thương hiệu");
        
        var campaign = await _unitOfWork.GetRepository<Campaigns>().SingleOrDefaultAsync(
            predicate: x => x.Id == request.CampaignId && x.BrandId == brandId,
            include: x => x.Include(x => x.CampaignRuleLinks)
        );
        if (campaign == null)
        {
            throw new BadHttpRequestException("Không tìm thấy chiến dịch khuyến mãi.");
        }

        if(request.PromotionRuleIds != null && request.PromotionRuleIds.Any())
        {
            var promotionRules = await _unitOfWork.GetRepository<PromotionRules>().GetListAsync(
                predicate: x => request.PromotionRuleIds.Contains(x.Id) && x.BrandId == brandId
            );
            if (promotionRules.Count() != request.PromotionRuleIds.Count)
            {
                throw new BadHttpRequestException("Một hoặc nhiều quy tắc khuyến mãi không hợp lệ hoặc không thuộc thương hiệu này.");
            }
        }
        
        var existingPromotionRuleIds = campaign.CampaignRuleLinks.Select(x => x.PromotionRuleId).ToHashSet();
        var newPromotionRules = new HashSet<Guid>(request.PromotionRuleIds);
        newPromotionRules.ExceptWith(existingPromotionRuleIds);
        var removePromotionRuleIds = new HashSet<Guid>(existingPromotionRuleIds);
        removePromotionRuleIds.ExceptWith(request.PromotionRuleIds);
        if(!newPromotionRules.Any() && !removePromotionRuleIds.Any())
        {
            _logger.Information("Không có quy tắc khuyến mãi nào được thêm hoặc xóa");
            return new ApiResponse()
            {
                Status = StatusCodes.Status200OK,
                Message = "Không có quy tắc khuyến mãi nào được thêm hoặc xóa",
                Data = campaign.Id,
            };
        }
        if (newPromotionRules.Any())
        {
            var newCampaignRuleLinks = newPromotionRules.Select(ruleId => new CampaignRuleLinks
            {
                Id = Guid.CreateVersion7(),
                CampaignId = campaign.Id,
                PromotionRuleId = ruleId
            }).ToList();
            await _unitOfWork.GetRepository<CampaignRuleLinks>().InsertRangeAsync(newCampaignRuleLinks);
        }
        if (removePromotionRuleIds.Any())
        {
            var campaignRuleLinksToRemove = await _unitOfWork.GetRepository<CampaignRuleLinks>().GetListAsync(
                predicate: x => removePromotionRuleIds.Contains(x.PromotionRuleId) && x.CampaignId == campaign.Id
            );
            if (campaignRuleLinksToRemove.Any())
            {
                _unitOfWork.GetRepository<CampaignRuleLinks>().DeleteRangeAsync(campaignRuleLinksToRemove);
            }
        }
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (!isSuccess)
        {
            _logger.Error("Cập nhật quy tắc khuyến mãi cho chiến dịch thất bại: {CampaignId}", campaign.Id);
            throw new Exception("Cập nhật quy tắc khuyến mãi cho chiến dịch thất bại");
        }
        _logger.Information("Cập nhật quy tắc khuyến mãi cho chiến dịch thành công: {CampaignId}", campaign.Id);
        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Cập nhật quy tắc khuyến mãi cho chiến dịch thành công",
            Data = campaign.Id
        };
    }
}