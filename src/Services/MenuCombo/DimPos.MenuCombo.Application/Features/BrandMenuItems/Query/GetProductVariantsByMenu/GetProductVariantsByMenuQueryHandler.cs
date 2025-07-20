using DimPos.Catalog.Application.Common.Protos;
using DimPos.MenuCombo.Application.Services.Interface;
using DimPos.MenuCombo.Domain.Entities;
using DimPos.MenuCombo.Domain.Enums;
using DimPos.MenuCombo.Domain.Models.Common;
using DimPos.MenuCombo.Domain.Models.ProductVariant;
using DimPos.MenuCombo.Infrastructure.Paginate;
using DimPos.MenuCombo.Infrastructure.Paginate.Interface;
using DimPos.MenuCombo.Infrastructure.Persistence;
using DimPos.MenuCombo.Infrastructure.Repositories.Interface;
using Mediator;
using Microsoft.EntityFrameworkCore;
using ProductVariantResponse = DimPos.MenuCombo.Domain.Models.ProductVariant.ProductVariantResponse;

namespace DimPos.MenuCombo.Application.Features.BrandMenuItems.Query.GetProductVariantsByMenu;

public class GetProductVariantsByMenuQueryHandler : IRequestHandler<GetProductVariantsByMenuQuery, ApiResponse>
{
    private readonly IUnitOfWork<MenuComboContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    private readonly CatalogGrpcService.CatalogGrpcServiceClient _catalogGrpcService;
    public GetProductVariantsByMenuQueryHandler(IUnitOfWork<MenuComboContext> unitOfWork,
        ILogger logger, IClaimService claimService, CatalogGrpcService.CatalogGrpcServiceClient catalogGrpcService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _claimService = claimService;
        _catalogGrpcService = catalogGrpcService;
    }
    
    public async ValueTask<ApiResponse> Handle(GetProductVariantsByMenuQuery request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if (brandId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy BrandId");
        }

        var brandMenuItems = await _unitOfWork.GetRepository<Domain.Entities.BrandMenuItems>().GetPagingListAsync(
            predicate: x => x.Menu.BrandId == brandId && 
                            x.Menu.Id == request.BrandMenuId,
            page: request.Page,
            size: request.Size,
            sortBy: request.SortBy ?? "DisplayOrder",
            isAsc: request.IsAsc
        );
        if (!brandMenuItems.Items.Any())
        {
            return new ApiResponse()
            {
                Status = 200,
                Message = "Không có sản phẩm nào trong menu này",
                Data = new Paginate<ProductVariantResponse>()
                {
                    Page = request.Page,
                    Size = request.Size,
                    Items = new List<ProductVariantResponse>(),
                    Total = 0,
                    TotalPages = 0
                }
            };
        }
        var productVariantIds = brandMenuItems.Items
            .Select(x => x.ProductVariantId.ToString())
            .ToList();
        
        var productVariantsGrpcResponse = await _catalogGrpcService.GetProductVariantListByIdsAsync(
            new GetProductVariantListByIdsRequest()
            {
                BrandId = brandId.ToString(),
                ProductVariantIds = { productVariantIds }
            });
        var response = productVariantsGrpcResponse.ProductVariants
            .Select(x => new ProductVariantResponse()
            {
                Id = Guid.Parse(x.Id),
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                IsActive = x.IsActive,
                Size = x.Size,
                Sku = x.Sku,
                Price = (decimal) x.Price
            }).ToList();
        return new ApiResponse()
        {
            Status = 200,
            Message = "Lấy danh sách sản phẩm trong menu thành công",
            Data = new Paginate<ProductVariantResponse>()
            {
                Page = brandMenuItems.Page,
                Size = brandMenuItems.Size,
                Items = response,
                Total = brandMenuItems.Total,
                TotalPages = brandMenuItems.TotalPages
            }
        };
    }
}