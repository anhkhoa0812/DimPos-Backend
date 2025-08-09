using DimPos.Promotion.Application.Services.Interface;
using DimPos.Promotion.Domain.Entities;
using DimPos.Promotion.Domain.Models.Common;
using DimPos.Promotion.Infrastructure.Persistence;
using DimPos.Promotion.Infrastructure.Repositories.Interface;
using DimPos.Store.Application.Common.Protos;
using Mediator;

namespace DimPos.Promotion.Application.Features.CampaignStore.Command;

public class CreateCampaignStoreCommandHandler : IRequestHandler<CreateCampaignStoreCommand, ApiResponse>
{
    private readonly IUnitOfWork<PromotionContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    private readonly StoreGrpcService.StoreGrpcServiceClient _storeGrpcService;
    
    public CreateCampaignStoreCommandHandler(IUnitOfWork<PromotionContext> unitOfWork, ILogger logger, 
        IClaimService claimService, StoreGrpcService.StoreGrpcServiceClient storeGrpcService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
        _storeGrpcService = storeGrpcService ?? throw new ArgumentNullException(nameof(storeGrpcService));
    }
    
    public async ValueTask<ApiResponse> Handle(CreateCampaignStoreCommand request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if (brandId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy Id thương hiệu");
        }

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
        var newCampaignStores = new List<CampaignStores>();
        foreach (var storeId in request.StoreIds)
        {
            var campaignStore = new CampaignStores()
            {
                Id = Guid.CreateVersion7(),
                CampaignId = request.CampaignId,
                StoreId = storeId,
            };
            newCampaignStores.Add(campaignStore);
        }
        await _unitOfWork.GetRepository<CampaignStores>().InsertRangeAsync(newCampaignStores);
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (!isSuccess)
        {
            throw new BadHttpRequestException("Tạo cửa hàng cho chiến dịch không thành công");
        }

        return new ApiResponse()
        {
            Status = StatusCodes.Status201Created,
            Message = "Tạo cửa hàng cho chiến dịch thành công",
            Data = null
        };
    }
}