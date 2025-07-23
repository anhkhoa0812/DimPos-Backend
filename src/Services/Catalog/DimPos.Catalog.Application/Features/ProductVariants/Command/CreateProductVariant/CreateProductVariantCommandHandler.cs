using DimPos.Catalog.Application.Services.Interface;
using DimPos.Catalog.Domain.Entities;
using DimPos.Catalog.Domain.Models.Common;
using DimPos.Catalog.Infrastructure.Persistence;
using DimPos.Catalog.Infrastructure.Repositories.Interface;
using DimPos.Catalog.Infrastructure.Utils;
using Mediator;

namespace DimPos.Catalog.Application.Features.ProductVariants.Command.CreateProductVariant;

public class CreateProductVariantCommandHandler : IRequestHandler<CreateProductVariantCommand, ApiResponse>
{
    private readonly IUnitOfWork<CatalogContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    
    public CreateProductVariantCommandHandler(IUnitOfWork<CatalogContext> unitOfWork, ILogger logger, IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(CreateProductVariantCommand request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if(brandId == Guid.Empty)
            throw new BadHttpRequestException("Không tìm thấy Id của thương hiệu");

        var accountId = _claimService.GetCurrentUserId;
        if(accountId == Guid.Empty)
            throw new BadHttpRequestException("Không tìm thấy Id của người dùng hiện tại");
        
        var product = await _unitOfWork.GetRepository<Domain.Entities.Products>().SingleOrDefaultAsync(
            predicate: x => x.Id == request.ProductId && x.BrandId == brandId 
                                                      && x.IsHasVariants
        );
        if (product == null)
        {
            throw new BadHttpRequestException("Không tìm thấy sản phẩm hoặc sản phẩm không có biến thể");
        }
        
        var productVariant = new Domain.Entities.ProductVariants
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            DisplayOrder = request.DisplayOrder,
            Price = request.Price,
            Sku = request.Sku,
            Size = request.Size,
            IsActive = true
        };
        await _unitOfWork.GetRepository<Domain.Entities.ProductVariants>().InsertAsync(productVariant);
        
        var newBasePrice = new BasePrice()
        {
            Id = Guid.CreateVersion7(),
            ProductVariantId = productVariant.Id,
            CurrencyCode = "VND",
            Price = request.Price,
            BrandId = brandId,
            BrandPriceHistories = new List<BrandPriceHistory>()
            {
                new BrandPriceHistory()
                {
                    Id = Guid.CreateVersion7(),
                    OldPrice = 0,
                    NewPrice = productVariant.Price,
                    ChangedAt = TimeUtil.GetCurrentSEATime(),
                    ChangedBy = accountId,
                    ProductVariantId = productVariant.Id,
                    CurrencyCode = "VND"
                }
            }
        };
        await _unitOfWork.GetRepository<BasePrice>().InsertAsync(newBasePrice);

        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (!isSuccess)
        {
            throw new BadHttpRequestException("Không thể tạo biến thể sản phẩm");
        }

        return new ApiResponse()
        {
            Status = StatusCodes.Status201Created,
            Message = "Tạo biến thể sản phẩm thành công",
            Data = productVariant.Id
        };
    }
}