using DimPos.Catalog.Application.Services.Interface;
using DimPos.Catalog.Domain.Enums;
using DimPos.Catalog.Domain.Models.Common;
using DimPos.Catalog.Domain.Models.InternalProducts;
using DimPos.Catalog.Infrastructure.Persistence;
using DimPos.Catalog.Infrastructure.Repositories.Interface;
using DimPos.Catalog.Infrastructure.Utils;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace DimPos.Catalog.Application.Features.InternalProducts.Query.GetInternalProducts;

public class GetInternalProductsQueryHandler : IRequestHandler<GetInternalProductsQuery, ApiResponse>
{
    private readonly IUnitOfWork<CatalogContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    
    public GetInternalProductsQueryHandler(IUnitOfWork<CatalogContext> unitOfWork, ILogger logger, IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(GetInternalProductsQuery request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if (brandId == Guid.Empty)
            throw new BadHttpRequestException("Không tìm thấy Id của thương hiệu");

        _logger.Information($"BEGIN: {nameof(GetInternalProductsQueryHandler)}: {TimeUtil.GetCurrentSEATime()}");
        var internalProducts = await _unitOfWork.GetRepository<Domain.Entities.ProductVariants>().GetPagingListAsync(
            selector: x => new GetInternalProductResponse()
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                IsActive = x.IsActive,
                DisplayOrder = x.DisplayOrder,
                Price = x.Price,
                Sku = x.Sku,
                ProductImages = x.Product.ProductImages != null ?
                    x.Product.ProductImages.Select(pi => new ProductImageForGetInternalProductResponse()
                    {
                        Id = pi.Id,
                        IsMainImage = pi.IsMainImage,
                        ImageUrl = pi.ImageUrl,
                        AltText = pi.AltText
                    }).ToList() 
                    : new List<ProductImageForGetInternalProductResponse>()
            },
            predicate: x => x.Product.BrandId == brandId && x.Product.Type == EProductType.InternalOrder
                && (request.Name == null || x.Name.Contains(request.Name)) 
                && (request.Sku == null || x.Sku.Contains(request.Sku)) 
                && (request.Code == null || x.Code.Contains(request.Code)),
            page: request.Page,
            size: request.Size,
            sortBy: request.SortBy ?? "DisplayOrder",
            isAsc: request.IsAsc,
            include: x => x.Include(x => x.Product)
        );
        _logger.Information($"END: {nameof(GetInternalProductsQueryHandler)}: {TimeUtil.GetCurrentSEATime()}");
        return new ApiResponse
        {
            Data = internalProducts,
            Status = (int) StatusCodes.Status200OK,
            Message = "Lấy dữ liệu thành công"
        };
    }
}