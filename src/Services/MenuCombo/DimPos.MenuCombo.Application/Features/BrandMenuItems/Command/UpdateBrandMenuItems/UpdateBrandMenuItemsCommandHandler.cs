using Confluent.Kafka;
using DimPos.Catalog.Application.Common.Protos;
using DimPos.MenuCombo.Application.Services.Interface;
using DimPos.MenuCombo.Domain.Entities;
using DimPos.MenuCombo.Domain.Models.Common;
using DimPos.MenuCombo.Infrastructure.Persistence;
using DimPos.MenuCombo.Infrastructure.Repositories.Interface;
using Google.Protobuf.Collections;
using MassTransit;
using Mediator;
using Microsoft.EntityFrameworkCore;
using SharedProject.Events.UpdateBrandMenuItem;

namespace DimPos.MenuCombo.Application.Features.BrandMenuItems.Command.UpdateBrandMenuItems;

public class UpdateBrandMenuItemsCommandHandler : IRequestHandler<UpdateBrandMenuItemsCommand, ApiResponse>
{
    private readonly IUnitOfWork<MenuComboContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    private readonly CatalogGrpcService.CatalogGrpcServiceClient _catalogGrpcService;
    private readonly ITopicProducer<Null, UpdateBrandMenuItemResponseModel> _topicProducer;
    public UpdateBrandMenuItemsCommandHandler(IUnitOfWork<MenuComboContext> unitOfWork,
        ILogger logger, IClaimService claimService,
        CatalogGrpcService.CatalogGrpcServiceClient catalogGrpcService,
        ITopicProducer<Null, UpdateBrandMenuItemResponseModel> topicProducer)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
        _catalogGrpcService = catalogGrpcService ?? throw new ArgumentNullException(nameof(catalogGrpcService));
        _topicProducer = topicProducer ?? throw new ArgumentNullException(nameof(topicProducer));
    }
    
    public async ValueTask<ApiResponse> Handle(UpdateBrandMenuItemsCommand request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if (brandId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy brandId");
        }
        var brandMenu = await _unitOfWork.GetRepository<Domain.Entities.BrandMenu>().SingleOrDefaultAsync(
            predicate: x => x.BrandId == brandId && x.Id == request.BrandMenuId,
            include: x => x.Include(x => x.StoreMenuAssignments)
        );
        if (brandMenu == null)
        {
            throw new BadHttpRequestException("Không tìm thấy BrandMenu");
        }

        var updateBrandMenuItemResponse = new UpdateBrandMenuItemResponseModel();
        
        var requestedIdStrings = request.UpdateBrandMenuItemsRequest.ProductVariantIds
            .Select(x => x.ToString())
            .ToList();
        var isValidProductVariants = _catalogGrpcService.CheckProductVariantInBrand(
            new CheckProductVariantInBrandRequest()
            {
                BrandId = brandId.ToString(),
                ListProductVariantId = new ListProductVariantId()
                {
                    ProductVariantId =
                    {
                        requestedIdStrings
                    }
                }
            }
        );
        if (!isValidProductVariants.IsValid)
        {
            throw new BadHttpRequestException("Một hoặc nhiều sản phẩm không hợp lệ");
        }
        var existingProductVariants = await _unitOfWork.GetRepository<Domain.Entities.BrandMenuItems>().GetListAsync(
            selector: x =>  x.ProductVariantId,
            predicate: x => x.MenuId == request.BrandMenuId && x.ProductVariantId != null
        );
        
        var existingProductVariantsSet = existingProductVariants.ToHashSet();
        var newProductVariantIds = new HashSet<Guid>(request.UpdateBrandMenuItemsRequest.ProductVariantIds);
        newProductVariantIds.ExceptWith(existingProductVariants);
        var removeProductVariantIds = new HashSet<Guid>(existingProductVariantsSet);
        removeProductVariantIds.ExceptWith(request.UpdateBrandMenuItemsRequest.ProductVariantIds);
        
        if (!newProductVariantIds.Any() && !removeProductVariantIds.Any())
        {
            throw new BadHttpRequestException("Không có sản phẩm nào để cập nhật");
        }
        
        if (newProductVariantIds.Any())
        {
            var newBrandMenuItems = new List<Domain.Entities.BrandMenuItems>();
            foreach (var productVariantId in newProductVariantIds)
            {
                var brandMenuItem = new Domain.Entities.BrandMenuItems()
                {
                    Id = Guid.CreateVersion7(),
                    MenuId = request.BrandMenuId,
                    ProductVariantId = productVariantId,
                    DisplayOrder = 0,
                    Description = null,
                };
                newBrandMenuItems.Add(brandMenuItem);
                updateBrandMenuItemResponse.NewProductVariantIds?.Add(productVariantId);
            }
            await _unitOfWork.GetRepository<Domain.Entities.BrandMenuItems>().InsertRangeAsync(newBrandMenuItems);
            
        }

        if (removeProductVariantIds.Any())
        {
            var removeBrandMenuItem = await _unitOfWork.GetRepository<Domain.Entities.BrandMenuItems>().GetListAsync(
                predicate: x => x.MenuId == request.BrandMenuId && removeProductVariantIds.Contains(x.ProductVariantId)
            );
            var storeMenuItemAvailability = await _unitOfWork.GetRepository<StoreMenuItemAvailability>().GetListAsync(
                predicate: x => removeBrandMenuItem.Select(x => x.Id).Contains(x.BrandMenuItemId)
            );
            _unitOfWork.GetRepository<StoreMenuItemAvailability>().DeleteRangeAsync(storeMenuItemAvailability);
            _unitOfWork.GetRepository<Domain.Entities.BrandMenuItems>().DeleteRangeAsync(removeBrandMenuItem);
            updateBrandMenuItemResponse.RemovedProductVariantIds?.AddRange(removeProductVariantIds);
        }

        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (!isSuccess)
        {
            throw new Exception("Câp nhật thất bại");
        }
        var storeIds = brandMenu.StoreMenuAssignments?.Select(x => x.StoreId).ToList();
        if (storeIds != null && storeIds.Any())
        {
            if(updateBrandMenuItemResponse.NewProductVariantIds != null || updateBrandMenuItemResponse.RemovedProductVariantIds != null)
            {
                updateBrandMenuItemResponse.BrandId = brandId;
                updateBrandMenuItemResponse.StoreIds = storeIds;
                updateBrandMenuItemResponse.CorrelationId = Guid.CreateVersion7();
                await _topicProducer.Produce(
                    key: null,
                    updateBrandMenuItemResponse,
                    cancellationToken
                ).ConfigureAwait(false);
            }
        }
        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Cập nhật thành công",
            Data = null
        };
    }
}