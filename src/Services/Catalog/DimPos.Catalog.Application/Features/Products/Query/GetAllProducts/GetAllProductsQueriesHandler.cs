using System.Net;
using DimPos.Catalog.Application.Common.Mapper;
using DimPos.Catalog.Application.Services.Interface;
using DimPos.Catalog.Domain.Models.Common;
using DimPos.Catalog.Domain.Models.Product;
using DimPos.Catalog.Infrastructure.Persistence;
using DimPos.Catalog.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.Catalog.Application.Features.Products.Query.GetAllProducts;

public class GetAllProductsQueriesHandler : IRequestHandler<GetAllProductsQueries, ApiResponse>
{
    private readonly IUnitOfWork<CatalogContext> _unitOfWork;
    private readonly IClaimService _claimService;

    public GetAllProductsQueriesHandler(IUnitOfWork<CatalogContext> unitOfWork, IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    public async ValueTask<ApiResponse> Handle(GetAllProductsQueries request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if (brandId == Guid.Empty)
            throw new BadHttpRequestException("Không tìm thấy Id của thương hiệu");
        var products = await _unitOfWork.GetRepository<Domain.Entities.Products>().GetPagingListAsync(
            selector: p => new ProductResponse()
            {
                Id = p.Id,
                Code = p.Code,
                AlternativeCode = p.AlternativeCode,
                Name = p.Name,
                Description = p.Description,
                IsHasVariants = p.IsHasVariants,
                IsHasRecipe = p.IsHasRecipe,
                Status = p.Status,
                IsAvailable = p.IsAvailable,
                DisplayOrder = p.DisplayOrder,
                IsMenuDisplay = p.IsMenuDisplay,
                SaleType = p.SaleType,
                IsMostOrdered = p.IsMostOrdered,
                Note = p.Note,
                NumOfUserVoted = p.NumOfUserVoted,
                CreatedDate = p.CreatedDate,
                LastModifiedDate = p.LastModifiedDate,
                ProductVariants = p.ProductVariants.Select(v => new ProductVariantsResponse
                {
                    Id = v.Id,
                    Code = v.Code,
                    AlternativeCode = v.AlternativeCode,
                    Name = v.Name,
                    DiscountPercent = v.DiscountPercent,
                    DiscountPrice = v.DiscountPrice,
                    Price = v.Price,
                    PriceCOGS = v.PriceCOGS,
                    IsActive = v.IsActive,
                    Size = v.Size,
                    IsMenuDisplay = v.IsMenuDisplay,
                    Sku = v.Sku,
                    Status = v.Status
                }).ToList(),
                ProductImages = p.ProductImages.Select(i => new ProductImagesResponse
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl,
                    IsMainImage = i.IsMainImage,
                    AltText = i.AltText
                }).ToList()
            },
            predicate: x => x.BrandId == brandId && 
                           (request.Status == null || x.Status == request.Status) &&
                           (string.IsNullOrEmpty(request.Name) || x.Name.Contains(request.Name)) &&
                           (request.IsHasVariants == null || x.IsHasVariants == request.IsHasVariants),
            page: request.Page,
            size: request.Size,
            sortBy: request.SortBy,
            isAsc: request.IsAsc
        );
        return new ApiResponse
        {
            Status = (int) HttpStatusCode.OK,
            Message = "Lấy dữ liệu thành công",
            Data = products
        };
    }
}