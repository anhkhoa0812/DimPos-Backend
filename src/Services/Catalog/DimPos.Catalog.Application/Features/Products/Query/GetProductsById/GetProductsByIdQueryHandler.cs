using DimPos.Catalog.Application.Common.Exceptions;
using DimPos.Catalog.Application.Services.Interface;
using DimPos.Catalog.Domain.Models.Common;
using DimPos.Catalog.Domain.Models.Product;
using DimPos.Catalog.Infrastructure.Persistence;
using DimPos.Catalog.Infrastructure.Repositories.Interface;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace DimPos.Catalog.Application.Features.Products.Query.GetProductsById;

public class GetProductsByIdQueryHandler : IRequestHandler<GetProductsByIdQuery, ApiResponse>
{
    private readonly IUnitOfWork<CatalogContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    
    public GetProductsByIdQueryHandler(IUnitOfWork<CatalogContext> unitOfWork, ILogger logger, IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(GetProductsByIdQuery request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if(brandId == Guid.Empty) 
            throw new BadHttpRequestException("Không tìm thấy thông tin thương hiệu trong yêu cầu.");
        _logger.Information("BEGIN: GetProductsByIdQueryHandler.Handle - ProductId: {ProductId}", request.ProductId);
        var product = await _unitOfWork.GetRepository<Domain.Entities.Products>().SingleOrDefaultAsync(
            predicate: x => x.Id == request.ProductId && x.BrandId == brandId,
            selector: p => new ProductByIdResponse()
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
                ProductImages = p.ProductImages.Select(pi => new ProductImagesResponse()
                {
                    Id = pi.Id,
                    ImageUrl = pi.ImageUrl,
                    IsMainImage = pi.IsMainImage,
                    AltText = pi.AltText
                }).ToList(),
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
                Category =  new CategoryResponse
                {
                    Id = p.Category.Id,
                    Code = p.Category.Code,
                    Name = p.Category.Name,
                    Description = p.Category.Description,
                    DisplayOrder = p.Category.DisplayOrder,
                    HasChildCategory = p.Category.HasChildCategory,
                    PictureUrl = p.Category.PictureUrl,
                    Status = p.Category.Status,
                    Type = p.Category.Type
                },
            }
        );
        if (product == null)
        {
            _logger.Error("Product not found with ID: {ProductId}", request.ProductId);
            throw new NotFoundException("Không tìm thấy sản phẩm với ID đã cung cấp.");
        }
        _logger.Information("END: GetProductsByIdQueryHandler.Handle - ProductId: {ProductId}", request.ProductId);
        
        return new ApiResponse
        {
            Status = 200,
            Message = "Lấy thông tin sản phẩm thành công.",
            Data = product
        };
    }
}