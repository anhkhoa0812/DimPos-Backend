using DimPos.Catalog.Application.Common.Exceptions;
using DimPos.Catalog.Application.Services.Interface;
using DimPos.Catalog.Domain.Enums;
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
            predicate: x => x.Id == request.ProductId && x.BrandId == brandId
            && x.Type == EProductType.CustomerOrder
            && !x.IsCombo,
            selector: p => new ProductByIdResponse()
            {
                Id = p.Id,
                Code = p.Code,
                Name = p.Name,
                Description = p.Description,
                IsHasVariants = p.IsHasVariants,
                DisplayOrder = p.DisplayOrder,
                Note = p.Note,
                CreatedDate = p.CreatedDate,
                LastModifiedDate = p.LastModifiedDate,
                ProductImages = p.ProductImages != null 
                    ? p.ProductImages.Select(pi => new ProductImagesResponse
                    {
                        Id = pi.Id,
                        ImageUrl = pi.ImageUrl,
                        IsMainImage = pi.IsMainImage,
                        AltText = pi.AltText
                    }).ToList()
                    : new List<ProductImagesResponse>(),
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
                ModifierGroup = p.ProductModifierGroups != null ?
                    p.ProductModifierGroups.Select(x => x.ModifierGroup).Select(x => new ModifierGroupResponse()
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Description = x.Description,
                        DisplayOrder = x.DisplayOrder,
                        IsActive = x.IsActive,
                        SelectedType = x.SelectedType
                    }).ToList() : new List<ModifierGroupResponse>()
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