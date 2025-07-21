using DimPos.Catalog.Application.Services.Interface;
using DimPos.Catalog.Domain.Entities;
using DimPos.Catalog.Domain.Models.Common;
using DimPos.Catalog.Domain.Models.StorePrices;
using DimPos.Catalog.Infrastructure.Paginate;
using DimPos.Catalog.Infrastructure.Persistence;
using DimPos.Catalog.Infrastructure.Repositories.Interface;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace DimPos.Catalog.Application.Features.StorePrices.Query.GetAllStorePricesByStoreId;

public class GetAllStorePricesByStoreIdQueryHandler : IRequestHandler<GetAllStorePricesByStoreIdQuery, ApiResponse>
{
    private readonly IUnitOfWork<CatalogContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    
    public GetAllStorePricesByStoreIdQueryHandler(IUnitOfWork<CatalogContext> unitOfWork, ILogger logger, IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(GetAllStorePricesByStoreIdQuery request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if (brandId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy id của thương hiệu");
        }

        var storePrices = await _unitOfWork.GetRepository<StorePrice>().GetPagingListAsync(
            predicate: x => x.StoreId == request.StoreId,
            page: request.Page,
            size: request.Size,
            sortBy: request.SortBy ?? "CreatedDate",
            isAsc: request.IsAsc
        );
        var response = new List<GetAllStorePricesByStoreIdResponse>();
        foreach (var storePrice in storePrices.Items)
        {
            var productVariant = await _unitOfWork.GetRepository<Domain.Entities.ProductVariants>()
                .SingleOrDefaultAsync(
                    predicate: x => x.Id == storePrice.ProductVariantId,
                    include: x => x.Include(x => x.Product)
                        .ThenInclude(x => x.ProductImages)
                );
            if (productVariant != null)
            {
                response.Add(new GetAllStorePricesByStoreIdResponse()
                {
                    Id = storePrice.Id,
                    CurrencyCode = storePrice.CurrencyCode,
                    OverridePrice = storePrice.OverridePrice,
                    ProductVariant = new ProductVariantByGetAllStorePricesByStoreIdResponse()
                    {
                        Id = productVariant.Id,
                        Name = productVariant.Name,
                        Code = productVariant.Code,
                        Description = productVariant.Description,
                        Size = productVariant.Size,
                        IsActive = productVariant.IsActive,
                        Price = productVariant.Price,
                        Sku = productVariant.Sku,
                        ImageUrl = productVariant.Product.ProductImages?.FirstOrDefault(x => x.IsMainImage)?.ImageUrl,
                    }
                });
            }
        }

        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Lấy danh sách giá của cửa hàng thành công",
            Data = new Paginate<GetAllStorePricesByStoreIdResponse>()
            {
                Page = storePrices.Page,
                Size = storePrices.Size,
                Total = storePrices.Total,
                Items = response,
                TotalPages = storePrices.TotalPages
            }
        };

    }
}