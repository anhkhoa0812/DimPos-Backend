using DimPos.Catalog.Application.Services.Interface;
using DimPos.Catalog.Domain.Enums;
using DimPos.Catalog.Domain.Models.ComboProducts;
using DimPos.Catalog.Domain.Models.Common;
using DimPos.Catalog.Infrastructure.Persistence;
using DimPos.Catalog.Infrastructure.Repositories.Interface;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace DimPos.Catalog.Application.Features.ComboProducts.Query.GetComboProductById;

public class GetComboProductByIdQueryHandler : IRequestHandler<GetComboProductByIdQuery, ApiResponse>
{
    private readonly IUnitOfWork<CatalogContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    
    public GetComboProductByIdQueryHandler(IUnitOfWork<CatalogContext> unitOfWork, ILogger logger, IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(GetComboProductByIdQuery request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if(brandId == Guid.Empty)
            throw new BadHttpRequestException("Không tìm thấy Id của thương hiệu");

        var productVariant = await _unitOfWork.GetRepository<Domain.Entities.ProductVariants>().SingleOrDefaultAsync(
            predicate: x => x.Id == request.ProductVariantId
            && x.Product.Type == EProductType.CustomerOrder
            && x.Product.IsCombo
            && !x.Product.IsExtra,
            include: x => x.Include(x => x.Product)
                .ThenInclude(x => x.ProductImages)
                .Include(x => x.Product.ProductComboItems)
        );
        if(productVariant == null)
            throw new BadHttpRequestException("Không tìm thấy biến thể sản phẩm với ID đã cung cấp.");

        var response = new GetComboProductByIdResponse()
        {
            Id = productVariant.Id,
            Code = productVariant.Code,
            Name = productVariant.Name,
            Description = productVariant.Description,
            IsActive = productVariant.IsActive,
            DisplayOrder = productVariant.DisplayOrder,
            Price = productVariant.Price,
            Sku = productVariant.Sku,
            ProductImages = productVariant.Product.ProductImages != null
                ? productVariant.Product.ProductImages.Select(pi => new ProductImageForGetComboProductByIdResponse()
                {
                    Id = pi.Id,
                    IsMainImage = pi.IsMainImage,
                    ImageUrl = pi.ImageUrl,
                    AltText = pi.AltText
                }).ToList()
                : new List<ProductImageForGetComboProductByIdResponse>(),
        };
        if (productVariant.Product.ProductComboItems != null)
        {
            foreach (var productComboItem in productVariant.Product.ProductComboItems)
            {
                var productVariantItem = await _unitOfWork.GetRepository<Domain.Entities.ProductVariants>()
                    .SingleOrDefaultAsync(
                        predicate: x => x.Id == productComboItem.ItemProductVariantId
                    );
                response.ComboProductItems.Add(new ComboProductItemResponse()
                {
                    Id = productComboItem.Id,
                    Quantity = productComboItem.Quantity,
                    DisplayOrder = productComboItem.DisplayOrder,
                    ProductVariant = new ProductVariantForComboProductItemResponse()
                    {
                        Id = productVariantItem.Id,
                        Name = productVariantItem.Name,
                        Code = productVariantItem.Code,
                        Description = productVariantItem.Description,
                        Price = productVariantItem.Price,
                        DisplayOrder = productVariantItem.DisplayOrder,
                        Sku = productVariantItem.Sku,
                    }
                });
            }
        }
        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Lấy dữ liệu thành công",
            Data = response
        };
    }
}