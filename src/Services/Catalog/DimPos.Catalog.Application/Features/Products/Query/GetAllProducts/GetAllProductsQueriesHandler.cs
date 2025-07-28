using System.Net;
using DimPos.Catalog.Application.Common.Mapper;
using DimPos.Catalog.Application.Services.Interface;
using DimPos.Catalog.Domain.Enums;
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
                Name = p.Name,
                Description = p.Description,
                IsHasVariants = p.IsHasVariants,
                DisplayOrder = p.DisplayOrder,
                Note = p.Note,
                IsActive = p.ProductVariants.Any(pv => pv.IsActive),
                CreatedDate = p.CreatedDate,
                LastModifiedDate = p.LastModifiedDate,
                ProductVariants = p.ProductVariants.Select(v => new ProductVariantsResponse
                {
                    Id = v.Id,
                    Code = v.Code,
                    Name = v.Name,
                    Description = v.Description,
                    Price = v.Price,
                    IsActive = v.IsActive,
                    Size = v.Size,
                    Sku = v.Sku,
                }).ToList(),
                ProductImages = p.ProductImages != null ? 
                    p.ProductImages.Select(i => new ProductImagesResponse
                    {
                        Id = i.Id,
                        ImageUrl = i.ImageUrl,
                        IsMainImage = i.IsMainImage,
                        AltText = i.AltText
                    }).ToList() : null
            },
            predicate: x => x.BrandId == brandId && 
                            x.Type == EProductType.CustomerOrder &&
                            !x.IsCombo &&
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