using DimPos.Promotion.Application.Services.Interface;
using DimPos.Promotion.Domain.Entities;
using DimPos.Promotion.Domain.Models.Common;
using DimPos.Promotion.Infrastructure.Persistence;
using DimPos.Promotion.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.Promotion.Application.Features.PromotionRule.Command.UpdatePromotionRule;

public class UpdatePromotionRuleCommandHandler : IRequestHandler<UpdatePromotionRuleCommand, ApiResponse>
{
    private readonly IUnitOfWork<PromotionContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    
    public UpdatePromotionRuleCommandHandler(IUnitOfWork<PromotionContext> unitOfWork, ILogger logger, IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(UpdatePromotionRuleCommand request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if(brandId == Guid.Empty)
            throw new BadHttpRequestException("Không tìm thấy thông tin thương hiệu trong yêu cầu.");

        var promotionRule = await _unitOfWork.GetRepository<PromotionRules>().SingleOrDefaultAsync(
            predicate: x => x.Id == request.PromotionRuleId && x.BrandId == brandId
        );
        if(promotionRule == null)
            throw new BadHttpRequestException("Không tìm thấy quy tắc khuyến mãi với ID đã cung cấp.");
        
        promotionRule.Name = request.Name ?? promotionRule.Name;
        promotionRule.Description = request.Description ?? promotionRule.Description;
        promotionRule.IsActive = request.IsActive ?? promotionRule.IsActive;
        promotionRule.Priority = request.Priority ?? promotionRule.Priority;
        promotionRule.ShortDescription = request.ShortDescription ?? promotionRule.ShortDescription;

        _unitOfWork.GetRepository<PromotionRules>().UpdateAsync(promotionRule);

        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if(!isSuccess)
            throw new Exception("Cập nhật quy tắc khuyến mãi không thành công.");

        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Cập nhật quy tắc khuyến mãi thành công",
            Data = null
        };
    }
}