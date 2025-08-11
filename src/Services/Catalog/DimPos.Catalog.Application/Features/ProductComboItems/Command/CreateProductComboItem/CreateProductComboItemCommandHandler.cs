using DimPos.Catalog.Application.Services.Interface;
using DimPos.Catalog.Domain.Enums;
using DimPos.Catalog.Domain.Models.Common;
using DimPos.Catalog.Infrastructure.Persistence;
using DimPos.Catalog.Infrastructure.Repositories.Interface;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace DimPos.Catalog.Application.Features.ProductComboItems.Command.CreateProductComboItem;

public class CreateProductComboItemCommandHandler : IRequestHandler<CreateProductComboItemCommand, ApiResponse>
{
    private readonly IUnitOfWork<CatalogContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    
    public CreateProductComboItemCommandHandler(IUnitOfWork<CatalogContext> unitOfWork, ILogger logger, IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(CreateProductComboItemCommand request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if (brandId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy id của thương hiệu");
        }

        var requestedProductVariant = await _unitOfWork.GetRepository<Domain.Entities.ProductVariants>()
            .SingleOrDefaultAsync(
                predicate: x => x.Id == request.ProductVariantItemId
            );
        if (requestedProductVariant == null)
        {
            throw new BadHttpRequestException("Không tìm thấy sản phẩm item");
        }
        var productVariant = await _unitOfWork.GetRepository<Domain.Entities.ProductVariants>().SingleOrDefaultAsync(
            predicate: x => x.Id == request.ProductVariantId 
                            && x.Product.BrandId == brandId
                            && x.Product.IsCombo 
                            && x.Product.Type == EProductType.CustomerOrder,
            include: x => x.Include(x => x.Product)
                .ThenInclude(x => x.ProductComboItems)
        );
        if (productVariant == null)
        {
            throw new BadHttpRequestException("Không tìm thấy sản phẩm combo");
        }

        var productVariantItem = await _unitOfWork.GetRepository<Domain.Entities.ProductVariants>()
            .SingleOrDefaultAsync(
                predicate: x => x.Id == request.ProductVariantItemId
                                && x.IsActive && x.Product.Type == EProductType.CustomerOrder
                                && !x.Product.IsCombo
            );
        if (productVariantItem == null)
        {
            throw new BadHttpRequestException("Không tìm thấy sản phẩm item với biến thể đã cung cấp hoặc sản phẩm item không hoạt động");
        }
        
        if (productVariant.Product.ProductComboItems != null 
            && productVariant.Product.ProductComboItems.Select(x => x.ItemProductVariantId).Distinct()
                .Contains(request.ProductVariantItemId))
        {
            throw new BadHttpRequestException("Sản phẩm combo item đã tồn tại trong combo này");
        }

        var productComboItem = new Domain.Entities.ProductComboItems()
        {
            Id = Guid.CreateVersion7(),
            ProductId = productVariant.Product.Id,
            ItemProductVariantId = productVariantItem.Id,
            Quantity = request.Quantity,
            DisplayOrder = request.DisplayOrder,
        };
        await _unitOfWork.GetRepository<Domain.Entities.ProductComboItems>().InsertAsync(productComboItem);
        
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (!isSuccess)
        {
            throw new BadHttpRequestException("Thêm sản phẩm combo item không thành công");
        }
        return new ApiResponse()
        {
            Status = StatusCodes.Status201Created,
            Message = "Thêm sản phẩm combo item thành công",
            Data = productComboItem.Id
        };
    }
}