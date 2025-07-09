using DimPos.Catalog.Application.Common.Protos;
using DimPos.Order.Application.Services.Interface;
using DimPos.Order.Domain.Entities;
using DimPos.Order.Domain.Enums;
using DimPos.Order.Domain.Models.Common;
using DimPos.Order.Infrastructure.Persistence;
using DimPos.Order.Infrastructure.Repositories.Interface;
using DimPos.Order.Infrastructure.Utils;
using DimPos.Store.Application.Common.Protos;
using Mediator;

namespace DimPos.Order.Application.Features.StorePurchaseOrder.Command.CreateStorePurchaseOrder;

public class CreateStorePurchaseOrderCommandHandler : IRequestHandler<CreateStorePurchaseOrderCommand, ApiResponse>
{
    private readonly IUnitOfWork<OrderContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    private readonly CatalogGrpcService.CatalogGrpcServiceClient _catalogGrpcService;
    private readonly StoreGrpcService.StoreGrpcServiceClient _storeGrpcService;
    
    public CreateStorePurchaseOrderCommandHandler(IUnitOfWork<OrderContext> unitOfWork, ILogger logger,
        IClaimService claimService, CatalogGrpcService.CatalogGrpcServiceClient catalogGrpcService,
        StoreGrpcService.StoreGrpcServiceClient storeGrpcService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
        _catalogGrpcService = catalogGrpcService ?? throw new ArgumentNullException(nameof(catalogGrpcService));
        _storeGrpcService = storeGrpcService ?? throw new ArgumentNullException(nameof(storeGrpcService));
    }
    
    public async ValueTask<ApiResponse> Handle(CreateStorePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var storeId = _claimService.GetStoreId ?? Guid.Empty;
        if (storeId == Guid.Empty)
            throw new BadHttpRequestException("Không tìm thấy Id của cửa hàng");
        
        var accountId = _claimService.GetCurrentUserId;
        if (accountId == Guid.Empty)
            throw new BadHttpRequestException("Không tìm thấy Id của tài khoản");

        var brandIdResponse = await _storeGrpcService.GetBrandIdByStoreIdAsync(new GetBrandIdByStoreIdRequest()
        {
            StoreId = storeId.ToString()
        });
        if (brandIdResponse.BrandId == String.Empty)
            throw new BadHttpRequestException("Không tìm thấy Id của thương hiệu liên kết với cửa hàng");
        var brandId = Guid.Parse(brandIdResponse.BrandId);
        _logger.Information("BEGIN: {StorePurchaseOrderCommandHandlerName} - {CurrentSeaTime}", nameof(CreateStorePurchaseOrderCommandHandler), TimeUtil.GetCurrentSEATime());
        var storePurchaseOrder = new StorePurchaseOrders()
        {
            Id = Guid.CreateVersion7(),
            StoreId = storeId,
            BrandId = brandId,
            Status = EStorePurchaseOrderStatus.New,
            NoteFromStore = request.Note,
            CreatedByAccountId = accountId,
        };
        var productVariantsGrpcResponse = await _catalogGrpcService.GetProductVariantForInternalOrderAsync(
            new GetProductVariantForInternalOrderRequest()
            {
                ProductVariantId =
                {
                    request.StorePurchaseOrderItems.Select(x => x.ProductVariantId.ToString()).ToList()
                },
                StoreId = storeId.ToString(),
                BrandId = brandId.ToString()
            }
        );
        foreach (var productVariant in productVariantsGrpcResponse.ProductVariants)
        {
            var requestProductVariant = request.StorePurchaseOrderItems
                .First(x => x.ProductVariantId == Guid.Parse(productVariant.ProductVariantId));
            
            var storePurchaseOrderItem = new StorePurchaseOrderItems()
            {
                Id = Guid.CreateVersion7(),
                StorePurchaseOrderId = storePurchaseOrder.Id,
                ProductVariantIdSnapshot = Guid.Parse(productVariant.ProductVariantId),
                ProductVariantNameSnapshot = productVariant.ProductVariantName,
                Quantity = requestProductVariant.Quantity,
                ProductVariantPriceSnapshot = (decimal) productVariant.ProductVariantPrice,
                TotalPriceOfOrderItems = requestProductVariant.Quantity * (decimal) productVariant.ProductVariantPrice,
            };
            
            storePurchaseOrder.StorePurchaseOrderItems.Add(storePurchaseOrderItem);
            storePurchaseOrder.EstimatedTotalValue += storePurchaseOrderItem.TotalPriceOfOrderItems;
        }

        await _unitOfWork.GetRepository<StorePurchaseOrders>().InsertAsync(storePurchaseOrder);
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        _logger.Information("END: {StorePurchaseOrderCommandHandlerName} - {CurrentSeaTime}", nameof(CreateStorePurchaseOrderCommandHandler), TimeUtil.GetCurrentSEATime());
        if (!isSuccess)
        {
            _logger.Error("Failed to create store purchase order for store {StoreId}", storeId);
            throw new Exception("Không thể tạo đơn hàng mua sắm cho cửa hàng");
        }
        return new ApiResponse
        {
            Data = storePurchaseOrder.Id,
            Status = (int) StatusCodes.Status200OK,
            Message = "Tạo đơn hàng mua sắm thành công"
        };
    }
}