using DimPos.Catalog.Application.Services.Interface;
using DimPos.Catalog.Domain.Enums;
using DimPos.Catalog.Domain.Models.Common;
using DimPos.Catalog.Domain.Models.InternalProducts;
using DimPos.Catalog.Infrastructure.Persistence;
using DimPos.Catalog.Infrastructure.Repositories.Interface;
using DimPos.Catalog.Infrastructure.Utils;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace DimPos.Catalog.Application.Features.InternalProducts.Query.GetInternalProductById;

public class GetInternalProductByIdQueryHandler : IRequestHandler<GetInternalProductByIdQuery, ApiResponse>
{
    private readonly IUnitOfWork<CatalogContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    
    public GetInternalProductByIdQueryHandler(IUnitOfWork<CatalogContext> unitOfWork, ILogger logger, IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(GetInternalProductByIdQuery request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if (brandId == Guid.Empty)
            throw new BadHttpRequestException("Không tìm thấy Id của thương hiệu");
        
        _logger.Information($"BEGIN: {nameof(GetInternalProductByIdQueryHandler)}: {request.ProductVariantId} - {TimeUtil.GetCurrentSEATime()}");
        
        var productVariant = await _unitOfWork.GetRepository<Domain.Entities.ProductVariants>().SingleOrDefaultAsync(
            predicate: x => x.Id == request.ProductVariantId 
                            && x.Product.BrandId == brandId 
                            && x.Product.Type == EProductType.InternalOrder,
            include: x => x.Include(x => x.Product)
        );
        if( productVariant == null)
            throw new BadHttpRequestException("Không tìm thấy biến thể sản phẩm với ID đã cung cấp.");

        var response = new GetInternalProductResponse()
        {
            Id = productVariant.Id,
            Code = productVariant.Code,
            Name = productVariant.Name,
            Description = productVariant.Description,
            IsActive = productVariant.IsActive,
            DisplayOrder = productVariant.DisplayOrder,
            Price = productVariant.Price,
            Sku = productVariant.Sku
        };

        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Lấy dữ liệu thành công",
            Data = response
        };
    }
}