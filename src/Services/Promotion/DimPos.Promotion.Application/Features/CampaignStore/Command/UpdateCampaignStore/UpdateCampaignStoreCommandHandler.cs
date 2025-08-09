using DimPos.Promotion.Application.Services.Interface;
using DimPos.Promotion.Domain.Entities;
using DimPos.Promotion.Domain.Models.Common;
using DimPos.Promotion.Infrastructure.Persistence;
using DimPos.Promotion.Infrastructure.Repositories.Interface;
using DimPos.Store.Application.Common.Protos;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace DimPos.Promotion.Application.Features.CampaignStore.Command.UpdateCampaignStore;

public class UpdateCampaignStoreCommandHandler : IRequestHandler<UpdateCampaignStoreCommand, ApiResponse>
{
    private readonly IUnitOfWork<PromotionContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    private readonly StoreGrpcService.StoreGrpcServiceClient _storeGrpcService;
    
    public UpdateCampaignStoreCommandHandler(IUnitOfWork<PromotionContext> unitOfWork, ILogger logger, IClaimService claimService,
        StoreGrpcService.StoreGrpcServiceClient storeGrpcService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
        _storeGrpcService = storeGrpcService ?? throw new ArgumentNullException(nameof(storeGrpcService));
    }
    
    public async ValueTask<ApiResponse> Handle(UpdateCampaignStoreCommand request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if (brandId == Guid.Empty)
            throw new  BadHttpRequestException("Không tìm thấy Id thương hiệu");

        var campaign = await _unitOfWork.GetRepository<Campaigns>().SingleOrDefaultAsync(
            predicate: x => x.Id == request.CampaignId && x.BrandId == brandId,
            include: x => x.Include(x => x.CampaignStores)
        );
        
        if (campaign == null)
            throw new BadHttpRequestException("Không tìm thấy chiến dịch khuyến mãi");
        
        if(request.StoreIds != null && request.StoreIds.Any())
        {
            var isValidStore = await _storeGrpcService.CheckStoresInBrandAsync(
                new CheckStoresInBrandRequest()
                {
                    BrandId = brandId.ToString(),
                    ListStoreId = new ListStoreId()
                    {
                        StoreId =
                        {
                            request.StoreIds.Select(x => x.ToString())
                        }
                    }
                }
            );
            if (!isValidStore.IsValid)
            {
                throw new BadHttpRequestException("Một hoặc nhiều cửa hàng không hợp lệ hoặc không thuộc thương hiệu này.");
            }
        }
        
        var existingStoreIds = campaign.CampaignStores?.Select(x => x.StoreId).ToHashSet();
        var newStoreIds = new HashSet<Guid>(request.StoreIds);
        newStoreIds.ExceptWith(existingStoreIds);
        var removeStoreIds = new HashSet<Guid>(existingStoreIds);
        removeStoreIds.ExceptWith(request.StoreIds);
        if (!newStoreIds.Any() && !removeStoreIds.Any())
        {
            _logger.Information("Không có thay đổi nào trong cửa hàng của chiến dịch khuyến mãi: {CampaignId}", campaign.Id);
            return new ApiResponse()
            {
                Status = StatusCodes.Status200OK,
                Message = "Không có thay đổi nào trong cửa hàng của chiến dịch khuyến mãi",
                Data = campaign.Id
            };
        }
        if (newStoreIds.Any())
        {
            var newCampaignStores = newStoreIds.Select(storeId => new CampaignStores
            {
                Id = Guid.CreateVersion7(),
                CampaignId = campaign.Id,
                StoreId = storeId,
            }).ToList();
            await _unitOfWork.GetRepository<CampaignStores>().InsertRangeAsync(newCampaignStores);
        }
        if (removeStoreIds.Any())
        {
            var campaignStoresToRemove = await _unitOfWork.GetRepository<CampaignStores>().GetListAsync(
                predicate: x => removeStoreIds.Contains(x.StoreId) && x.CampaignId == request.CampaignId
            );
            _unitOfWork.GetRepository<CampaignStores>().DeleteRangeAsync(campaignStoresToRemove);
        }
        
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (!isSuccess)
        {
            _logger.Error("Cập nhật cửa hàng cho chiến dịch khuyến mãi thất bại: {CampaignId}", campaign.Id);
            throw new Exception("Cập nhật cửa hàng cho chiến dịch khuyến mãi thất bại");
        }
        _logger.Information("Cập nhật cửa hàng cho chiến dịch khuyến mãi thành công: {CampaignId}", campaign.Id);
        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Cập nhật cửa hàng cho chiến dịch khuyến mãi thành công",
            Data = campaign.Id
        };
    }
}