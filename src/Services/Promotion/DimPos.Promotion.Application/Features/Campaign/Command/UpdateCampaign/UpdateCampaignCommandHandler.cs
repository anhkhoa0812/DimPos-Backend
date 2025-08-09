using DimPos.Promotion.Application.Services.Interface;
using DimPos.Promotion.Domain.Entities;
using DimPos.Promotion.Domain.Models.Common;
using DimPos.Promotion.Infrastructure.Persistence;
using DimPos.Promotion.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.Promotion.Application.Features.Campaign.Command.UpdateCampaign;

public class UpdateCampaignCommandHandler : IRequestHandler<UpdateCampaignCommand, ApiResponse>
{
    private readonly IUnitOfWork<PromotionContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    
    public UpdateCampaignCommandHandler(IUnitOfWork<PromotionContext> unitOfWork, ILogger logger, IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(UpdateCampaignCommand request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if (brandId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy Id thương hiệu");
        }

        var campaign = await _unitOfWork.GetRepository<Campaigns>().SingleOrDefaultAsync(
            predicate: x => x.Id == request.CampaignId && x.BrandId == brandId
        );

        if (campaign == null)
        {
            throw new BadHttpRequestException("Không tìm thấy chiến dịch khuyến mãi");
        }

        if (request.StartDate != null)
        {
            var endDate = request.EndDate ?? campaign.EndDate;
            if (request.StartDate > endDate)
            {
                throw new BadHttpRequestException("Ngày bắt đầu không thể lớn hơn ngày kết thúc");
            }
            campaign.StartDate = request.StartDate.Value;
        }
        if (request.EndDate != null)
        {
            var startDate = request.StartDate ?? campaign.StartDate;
            if (request.EndDate < startDate)
            {
                throw new BadHttpRequestException("Ngày kết thúc không thể nhỏ hơn ngày bắt đầu");
            }
            campaign.EndDate = request.EndDate.Value;
        }
        campaign.Name = !string.IsNullOrEmpty(request.Name) ? request.Name : campaign.Name;
        campaign.Description = !string.IsNullOrEmpty(request.Description) ? request.Description : campaign.Description;
        campaign.Priority = request.Priority ?? campaign.Priority;
        campaign.IsActive = request.IsActive ?? campaign.IsActive;
        
        _unitOfWork.GetRepository<Campaigns>().UpdateAsync(campaign);
        
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (!isSuccess)
        {
            _logger.Error("Cập nhật chiến dịch khuyến mãi thất bại: {CampaignId}", campaign.Id);
            throw new Exception("Cập nhật chiến dịch khuyến mãi thất bại");
        }
        _logger.Information("Cập nhật chiến dịch khuyến mãi thành công: {CampaignId}", campaign.Id);
        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Cập nhật chiến dịch khuyến mãi thành công",
            Data = campaign.Id
        };
    }
}