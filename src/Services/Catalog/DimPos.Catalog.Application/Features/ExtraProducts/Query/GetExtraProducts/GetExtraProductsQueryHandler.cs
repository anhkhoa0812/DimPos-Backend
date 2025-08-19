using DimPos.Catalog.Application.Services.Interface;
using DimPos.Catalog.Domain.Enums;
using DimPos.Catalog.Domain.Models.Common;
using DimPos.Catalog.Domain.Models.ExtraProducts;
using DimPos.Catalog.Infrastructure.Persistence;
using DimPos.Catalog.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.Catalog.Application.Features.ExtraProducts.Query.GetExtraProducts;

public class GetExtraProductsQueryHandler : IRequestHandler<GetExtraProductsQuery, ApiResponse>
{
    private readonly IUnitOfWork<CatalogContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    
    public GetExtraProductsQueryHandler(IUnitOfWork<CatalogContext> unitOfWork, ILogger logger, IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(GetExtraProductsQuery request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if (brandId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy Id của thương hiệu");
        }

        var extraProducts = await _unitOfWork.GetRepository<Domain.Entities.ProductVariants>().GetPagingListAsync(
            selector: x => new GetExtraProductsResponse()
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                IsActive = x.IsActive,
                DisplayOrder = x.DisplayOrder,
                Price = x.Price,
                Sku = x.Sku,
                ProductImages = x.Product.ProductImages != null
                    ? x.Product.ProductImages.Select(pi => new ProductImageForGetExtraProductsResponse()
                    {
                        Id = pi.Id,
                        IsMainImage = pi.IsMainImage,
                        ImageUrl = pi.ImageUrl,
                        AltText = pi.AltText
                    }).ToList()
                    : new List<ProductImageForGetExtraProductsResponse>()
            },
            predicate: x => x.Product.BrandId == brandId 
                            && !x.Product.IsCombo
                            && x.Product.IsExtra
                            && x.Product.Type == EProductType.CustomerOrder
                            && (string.IsNullOrEmpty(request.Name) || x.Name.Contains(request.Name)) 
                            && (string.IsNullOrEmpty(request.Sku) || x.Code.Contains(request.Sku)),
            page: request.Page,
            size: request.Size,
            sortBy: request.SortBy ?? "DisplayOrder",
            isAsc: request.IsAsc
        );
        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Lấy danh sách sản phẩm phụ thành công",
            Data = extraProducts
        };


    }
}