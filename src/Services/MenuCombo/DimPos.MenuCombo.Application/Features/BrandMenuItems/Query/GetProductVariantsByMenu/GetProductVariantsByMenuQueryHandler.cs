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

        var brandMenu = await _unitOfWork.GetRepository<Domain.Entities.BrandMenu>().SingleOrDefaultAsync(
            predicate: x => x.BrandId == brandId && x.Id == request.BrandMenuId
        );
        if (brandMenu == null)
        {
            throw new BadHttpRequestException("Không tìm thấy BrandMenu");
        }

        var productVariantsIdInBrandMenu = await _unitOfWork.GetRepository<Domain.Entities.BrandMenuItems>().GetListAsync(
            selector: x => x.ProductVariantId,
            predicate: x => x.MenuId == request.BrandMenuId
        );
        var productVariantsGrpcResponse = _catalogGrpcService.GetProductVariantsByBrand(
            new GetProductVariantsByBrandRequest()
            {
                BrandId = brandId.ToString(),
                Page = request.Page,
                PageSize = request.Size,
                IsAsc = request.IsAsc,
            });
        var response = new List<ProductVariantResponse>();
        foreach (var productVariantGrpcResponse in productVariantsGrpcResponse.ProductVariants)
        {
            response.Add(new ProductVariantResponse()
            { 
                Id = Guid.Parse(productVariantGrpcResponse.Id),
                Name = productVariantGrpcResponse.Name,
                Price = (decimal) productVariantGrpcResponse.Price,
                Code = productVariantGrpcResponse.Code,
                AlternativeCode = productVariantGrpcResponse.AlternativeCode,
                DiscountPercent = (decimal) productVariantGrpcResponse.DiscountPercent,
                DiscountPrice = (decimal) productVariantGrpcResponse.DiscountPrice,
                PriceCOGS = (decimal) productVariantGrpcResponse.PriceCOGS,
                IsActive = productVariantGrpcResponse.IsActive,
                Size = productVariantGrpcResponse.Size,
                DisplayOrder =  productVariantGrpcResponse.DisplayOrder,
                IsMenuDisplay = productVariantGrpcResponse.IsMenuDisplay,
                Status = (EProductVariantStatus) productVariantGrpcResponse.Status,
                IsSelected = productVariantsIdInBrandMenu.Contains(Guid.Parse(productVariantGrpcResponse.Id))
            });
        }
        return new ApiResponse()
        {
            Status = 200,
            Message = "Lấy dữ liệu thành công",
            Data = new Paginate<ProductVariantResponse>()
            {
                Page = request.Page,
                Size = request.Size,
                Items = response,
                Total = productVariantsGrpcResponse.Total,
                TotalPages = productVariantsGrpcResponse.TotalPages
            }
        };
    }
}