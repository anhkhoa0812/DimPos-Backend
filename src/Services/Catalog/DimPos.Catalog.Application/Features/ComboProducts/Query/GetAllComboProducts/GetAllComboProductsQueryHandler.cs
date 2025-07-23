using DimPos.Catalog.Application.Services.Interface;
using DimPos.Catalog.Domain.Enums;
using DimPos.Catalog.Domain.Models.ComboProducts;
using DimPos.Catalog.Domain.Models.Common;
using DimPos.Catalog.Infrastructure.Persistence;
using DimPos.Catalog.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.Catalog.Application.Features.ComboProducts.Query.GetAllComboProducts;

public class GetAllComboProductsQueryHandler : IRequestHandler<GetAllComboProductsQuery, ApiResponse>
{
    private readonly IUnitOfWork<CatalogContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    
    public GetAllComboProductsQueryHandler(IUnitOfWork<CatalogContext> unitOfWork, ILogger logger, IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(GetAllComboProductsQuery request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if (brandId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy brandId");
        }

        var comboProducts = await _unitOfWork.GetRepository<Domain.Entities.ProductVariants>().GetPagingListAsync(
            selector: x => new GetAllComboProductsResponse()
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
                    ? x.Product.ProductImages.Select(pi => new ProductImageForGetAllComboProductsResponse()
                    {
                        Id = pi.Id,
                        IsMainImage = pi.IsMainImage,
                        ImageUrl = pi.ImageUrl,
                        AltText = pi.AltText
                    }).ToList()
                    : new List<ProductImageForGetAllComboProductsResponse>()
            },
            predicate: x => x.Product.BrandId == brandId 
                            && x.Product.IsCombo 
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
            Message = "Lấy danh sách combo sản phẩm thành công",
            Data = comboProducts
        };
    }
}