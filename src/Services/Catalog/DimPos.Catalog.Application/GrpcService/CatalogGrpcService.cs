using DimPos.Catalog.Application.Common.Protos;
using DimPos.Catalog.Domain.Entities;
using DimPos.Catalog.Domain.Enums;
using DimPos.Catalog.Infrastructure.Persistence;
using DimPos.Catalog.Infrastructure.Repositories.Interface;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;

namespace DimPos.Catalog.Application.GrpcService;

public class CatalogGrpcService : Common.Protos.CatalogGrpcService.CatalogGrpcServiceBase
{
    private readonly IUnitOfWork<CatalogContext> _unitOfWork;
    private readonly ILogger _logger;
    
    public CatalogGrpcService(IUnitOfWork<CatalogContext> unitOfWork, ILogger logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }
    public override async Task<GetProductVariantsByBrandResponse> GetProductVariantsByBrand(GetProductVariantsByBrandRequest request, ServerCallContext context)
    {
        _logger.Information($"BEGIN: {nameof(GetProductVariantsByBrand)} - {DateTime.UtcNow}");
        var productVariantsPaging = await _unitOfWork.GetRepository<ProductVariants>().GetPagingListAsync(
            predicate: x => x.Product!.BrandId == Guid.Parse(request.BrandId), 
            page: request.Page,
            size: request.PageSize,
            isAsc: request.IsAsc,
            sortBy: request.SortBy,
            include: x => x.Include(x => x.Product)
        );
        var response = new GetProductVariantsByBrandResponse();
        response.Total = productVariantsPaging.Total;
        response.TotalPages = productVariantsPaging.TotalPages;
        if (productVariantsPaging != null)
        {
            foreach (var productVariant in productVariantsPaging.Items)
            {
                var productVariantResponse = new ProductVariant()
                {
                    Id = productVariant.Id.ToString(),
                    Code = productVariant.Code,
                    Name = productVariant.Name,
                    AlternativeCode = productVariant.AlternativeCode,
                    Status = (ProductVariantStatus) productVariant.Status,
                    IsActive = productVariant.IsActive ?? false,
                    IsMenuDisplay = productVariant.IsMenuDisplay ?? false,
                    DisplayOrder = productVariant.DisplayOrder ?? 0,
                    Price = (float) productVariant.Price,
                    DiscountPercent = (float) productVariant.DiscountPercent,
                    DiscountPrice = (float) productVariant.DiscountPrice,
                    PriceCOGS = (float) productVariant.PriceCOGS,
                };
                response.ProductVariants.Add(productVariantResponse);
            }
        }
        _logger.Information($"END: {nameof(GetProductVariantsByBrand)} - {DateTime.UtcNow}");
        return response;
    }

    public override async Task<CheckProductVariantInBrandResponse> CheckProductVariantInBrand(CheckProductVariantInBrandRequest request, ServerCallContext context)
    {
        _logger.Information($"BEGIN: {nameof(CheckProductVariantInBrand)} - {DateTime.UtcNow}");
        var productVariantIds = await _unitOfWork.GetRepository<ProductVariants>().GetListAsync(
            selector: x => x.Id,
            predicate: x => x.Product.BrandId == Guid.Parse(request.BrandId),
            include: x => x.Include(x => x.Product)
        );
        var requestedIds = request.ListProductVariantId.ProductVariantId
            .Select(Guid.Parse)
            .ToList();
        var variantSet = new HashSet<Guid>(productVariantIds);
        bool allExist = requestedIds.All(variantSet.Contains);
        _logger.Information($"END: {nameof(CheckProductVariantInBrand)} - {DateTime.UtcNow}");

        if (!allExist)
        {
            return new CheckProductVariantInBrandResponse()
            {
                IsValid = false,
            };
        }
        return new CheckProductVariantInBrandResponse()
        {
            IsValid = true
        };
        
    }
}