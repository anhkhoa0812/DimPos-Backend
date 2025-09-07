using DimPos.Catalog.Application.Services.Interface;
using DimPos.Catalog.Domain.Enums;
using DimPos.Catalog.Domain.Models.Common;
using DimPos.Catalog.Infrastructure.Persistence;
using DimPos.Catalog.Infrastructure.Repositories.Interface;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace DimPos.Catalog.Application.Features.ProductExtraItems.Command.UpdateProductExtraItem;

public class UpdateProductExtraItemCommandHandler : IRequestHandler<UpdateProductExtraItemCommand, ApiResponse>
{
    private readonly IUnitOfWork<CatalogContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    
    public UpdateProductExtraItemCommandHandler(IUnitOfWork<CatalogContext> unitOfWork, ILogger logger, IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(UpdateProductExtraItemCommand request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if (brandId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy Id của thương hiệu");
        }

        var product = await _unitOfWork.GetRepository<Domain.Entities.Products>().SingleOrDefaultAsync(
            predicate: x => x.Id == request.ProductId 
                            && x.BrandId == brandId
                            && !x.IsExtra
                            && !x.IsCombo
                            && x.Type == EProductType.CustomerOrder,
            include: x => x.Include(x => x.ProductExtraItems)
                .ThenInclude(x => x.ExtraProductVariant)
        );
        if (product == null)
        {
            throw new BadHttpRequestException("Không tìm thấy sản phẩm");
        }
        var existingProductVariants  = product.ProductExtraItems.Select(x => x.ExtraProductVariant.Id).ToList();
        var requestExtraProductIds = await _unitOfWork.GetRepository<Domain.Entities.ProductVariants>().GetListAsync(
            selector: x => x.Id,
            predicate: x => request.ProductVariantItemIds.Contains(x.Id)
                            && x.IsActive
                            && x.Product.Type == EProductType.CustomerOrder
                            && !x.Product.IsCombo
                            && x.Product.IsExtra
        );
        
        if (requestExtraProductIds.Count != request.ProductVariantItemIds.Count)
        {
            throw new BadHttpRequestException("Một hoặc nhiều sản phẩm extra không hợp lệ");
        }
        var existingProductVariantsSet = existingProductVariants.ToHashSet();
        var newProductVariantIds = new HashSet<Guid>(requestExtraProductIds);
        newProductVariantIds.ExceptWith(existingProductVariants);
        var removeProductVariantIds = new HashSet<Guid>(existingProductVariantsSet);
        removeProductVariantIds.ExceptWith(requestExtraProductIds);
        
        if (!newProductVariantIds.Any() && !removeProductVariantIds.Any())
        {
            throw new BadHttpRequestException("Không có sản phẩm nào để cập nhật");
        }
        
        if (newProductVariantIds.Any())
        {
            var newProductExtraItems = new List<Domain.Entities.ProductExtraItems>();
            foreach (var productVariantId in newProductVariantIds)
            {
                var productExtraItems = new Domain.Entities.ProductExtraItems()
                {
                    Id = Guid.CreateVersion7(),
                    ProductId = product.Id,
                    ExtraProductVariantId = productVariantId
                };
                newProductExtraItems.Add(productExtraItems);
            }
            await _unitOfWork.GetRepository<Domain.Entities.ProductExtraItems>().InsertRangeAsync(newProductExtraItems);
            
        }

        if (removeProductVariantIds.Any())
        {
            var removeProductExtraItems = await _unitOfWork.GetRepository<Domain.Entities.ProductExtraItems>().GetListAsync(
                predicate: x => x.Product.BrandId == brandId 
                                && x.ProductId == product.Id
                                && removeProductVariantIds.Contains(x.ExtraProductVariantId)
            );
            
            _unitOfWork.GetRepository<Domain.Entities.ProductExtraItems>().DeleteRangeAsync(removeProductExtraItems);
        }

        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (!isSuccess)
        {
            throw new BadHttpRequestException("Không thể thêm sản phẩm extra vào sản phẩm biến thể");
        }

        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Thêm sản phẩm extra thành công",
            Data = product.Id
        };
    }
}