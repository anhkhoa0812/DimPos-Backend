using DimPos.Catalog.Application.Services.Interface;
using DimPos.Catalog.Domain.Enums;
using DimPos.Catalog.Domain.Models.Common;
using DimPos.Catalog.Domain.Models.ExtraProducts;
using DimPos.Catalog.Infrastructure.Persistence;
using DimPos.Catalog.Infrastructure.Repositories.Interface;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace DimPos.Catalog.Application.Features.ExtraProducts.Query.GetExtraProductById;

public class GetExtraProductByIdQueryHandler : IRequestHandler<GetExtraProductByIdQuery, ApiResponse>
{
    private readonly IUnitOfWork<CatalogContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    
    public GetExtraProductByIdQueryHandler(IUnitOfWork<CatalogContext> unitOfWork, ILogger logger, IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(GetExtraProductByIdQuery request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if (brandId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy Id của thương hiệu");
        }

        var productVariant = await _unitOfWork.GetRepository<Domain.Entities.ProductVariants>().SingleOrDefaultAsync(
            predicate: x => x.Id == request.ProductVariantId 
                            && x.Product.Type == EProductType.CustomerOrder
                            && !x.Product.IsCombo
                            && x.Product.IsExtra,
            include: x => x.Include(x => x.Product)
                .ThenInclude(x => x.ProductImages)
                .Include(x => x.RecipeItems)
                .ThenInclude(x => x.Ingredient)
        );

        var response = new GetExtraProductByIdResponse()
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
            ? productVariant.Product.ProductImages.Select(pi => new ProductImageForGetExtraProductByIdResponse()
            {
                Id = pi.Id,
                IsMainImage = pi.IsMainImage,
                ImageUrl = pi.ImageUrl,
                AltText = pi.AltText
            }).ToList()
            : new List<ProductImageForGetExtraProductByIdResponse>(),
            RecipeItems = productVariant.RecipeItems != null ?
                productVariant.RecipeItems.Select(ri => new RecipeItemsForGetExtraProductByIdResponse()
                {
                    Id = ri.Id,
                    Quantity = ri.Quantity,
                    CreatedDate = ri.CreatedDate,
                    LastModifiedDate = ri.LastModifiedDate,
                    CreatedByAccountId = ri.CreatedByAccountId,
                    Ingredient = new IngredientForGetExtraProductByIdResponse()
                    {
                        Id = ri.Ingredient.Id,
                        Code = ri.Ingredient.Code,
                        Sku = ri.Ingredient.Sku,
                        Name = ri.Ingredient.Name,
                        MeasureUnit = ri.Ingredient.MeasureUnit,
                        IsActive = ri.Ingredient.IsActive,
                        CreatedDate = ri.CreatedDate,
                        LastModifiedDate = ri.LastModifiedDate,
                        Description = ri.Ingredient.Description
                    }
                }).ToList() : new List<RecipeItemsForGetExtraProductByIdResponse>()
        };

        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Lấy thông tin sản phẩm extra thành công",
            Data = response
        };
    }
}