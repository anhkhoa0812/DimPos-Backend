using System.Net;
using DimPos.Catalog.Application.Services.Interface;
using DimPos.Catalog.Domain.Entities;
using DimPos.Catalog.Domain.Enums;
using DimPos.Catalog.Domain.Models.Common;
using DimPos.Catalog.Domain.Models.ProductVariants;
using DimPos.Catalog.Infrastructure.Persistence;
using DimPos.Catalog.Infrastructure.Repositories.Interface;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace DimPos.Catalog.Application.Features.ProductVariants.Query.GetProductVariantsForMenu;

public class GetProductVariantsForMenuQueryHandler : IRequestHandler<GetProductVariantsForMenuQuery, ApiResponse>
{
    private readonly IUnitOfWork<CatalogContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    
    public GetProductVariantsForMenuQueryHandler(IUnitOfWork<CatalogContext> unitOfWork, ILogger logger, IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(GetProductVariantsForMenuQuery request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if (brandId == Guid.Empty)
            throw new BadHttpRequestException("Không tìm thấy Id của thương hiệu");

        var basePrices = await _unitOfWork.GetRepository<BasePrice>().GetListAsync(
            predicate: x => x.BrandId == brandId
        );
        var priceDict = basePrices.ToDictionary(x => x.ProductVariantId, x => x.Price);

        var productVariants = await _unitOfWork.GetRepository<Domain.Entities.ProductVariants>().GetPagingListAsync(
            selector: x => new GetProductVariantsResponse()
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                IsActive = x.IsActive,
                Size = x.Size,
                Sku = x.Sku
            },
            predicate: x => x.Product.BrandId == brandId &&
                            x.Product.Type == EProductType.CustomerOrder,
            page: request.Page,
            size: request.Size,
            sortBy: request.SortBy ?? "DisplayOrder",
            isAsc: request.IsAsc,
            include: x => x.Include(x => x.Product)
        );
        foreach (var item in productVariants.Items)
        {
            item.Price = priceDict.TryGetValue(item.Id, out var price) ? price : 0m;
        }
        return new ApiResponse()
        {
            Status = (int) HttpStatusCode.OK,
            Message = "Lấy dữ liệu thành công",
            Data = productVariants
        };
    }
}