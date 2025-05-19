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

    public override async Task<GetProductVariantsByBrandResponse> GetProductVariantsByBrand(
        GetProductVariantsByBrandRequest request, ServerCallContext context)
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
                    AlternativeCode = productVariant.AlternativeCode ?? String.Empty,
                    Status = (ProductVariantStatus) productVariant.Status,
                    IsActive = productVariant.IsActive,
                    IsMenuDisplay = productVariant.IsMenuDisplay ?? false,
                    DisplayOrder = productVariant.DisplayOrder ?? 0,
                    Price = (float)productVariant.Price,
                    DiscountPercent = (float)productVariant.DiscountPercent,
                    DiscountPrice = (float)productVariant.DiscountPrice,
                    PriceCOGS = (float)productVariant.PriceCOGS,
                };
                response.ProductVariants.Add(productVariantResponse);
            }
        }

        _logger.Information($"END: {nameof(GetProductVariantsByBrand)} - {DateTime.UtcNow}");
        return response;
    }

    public override async Task<CheckProductVariantInBrandResponse> CheckProductVariantInBrand(
        CheckProductVariantInBrandRequest request, ServerCallContext context)
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

    public override async Task<GetMenuProductByStoreResponse> GetMenuProductByStore(GetMenuProductByStoreRequest request, ServerCallContext context)
    {
        var variantIds = request.ListProductVariantIds.ProductVariantId.Select(Guid.Parse).ToList();
        var categories = await _unitOfWork.GetRepository<Categories>().GetListAsync(
            predicate: x =>
                x.Status == ECategoryStatus.Active &&
                x.Products.Any(p => p.ProductVariants
                    .Any(v => variantIds.Contains(v.Id) && v.Status == EProductVariantStatus.Active && v.IsActive == true)),
            include: x => x.Include(c => c.Products.Where(p => p.ProductVariants.Any(v => variantIds.Contains(v.Id))))
                .ThenInclude(p => p.ProductVariants.Where(v => variantIds.Contains(v.Id)))
        );
        var storePrices = await _unitOfWork.GetRepository<StorePrice>().GetListAsync(
            predicate: x => x.StoreId == Guid.Parse(request.StoreId)
            && variantIds.Contains(x.ProductVariantId)
        );
        var response = new GetMenuProductByStoreResponse();
        foreach (var category in categories)
        {
            var categoryItem = new CategoryResponse()
            {
                Id = category.Id.ToString(),
                Name = category.Name,
                DisplayOrder = category.DisplayOrder ?? 0,
                Code = category.Code ?? String.Empty,
                Description = category.Description ?? String.Empty,
            };
            foreach (var product in category.Products)
            {
                if (!product.IsHasVariants)
                {
                    var variant = product.ProductVariants.FirstOrDefault(pv => pv.ProductId == product.Id);
                    var productItem = new ProductResponse()
                    {
                        Id = variant.Id.ToString(),
                        Code = variant.Code,
                        Name = variant.Name,
                        AlternativeCode = variant.AlternativeCode ?? String.Empty,
                        ImageUrl = String.Empty,
                        Description = product.Description,
                        Price = (float) storePrices.FirstOrDefault(x => x.ProductVariantId == variant.Id).OverridePrice,
                        ProductVariants = null
                    };
                    categoryItem.Products.Add(productItem);
                }
                else
                {
                    var productItem = new ProductResponse()
                    {
                        Id = product.Id.ToString(),
                        Code = product.Code,
                        Name = product.Name,
                        AlternativeCode = product.AlternativeCode ?? String.Empty,
                        ImageUrl = "",
                        Description = product.Description,
                        Price = 0,
                        ProductVariants = new ListProductVariant()
                        {
                            ProductVariants =
                            {
                                product.ProductVariants?.Select(pv => new ProductVariantResponse()
                                {
                                    Id = pv.Id.ToString(),
                                    Code = pv.Code,
                                    Name = pv.Name,
                                    AlternativeCode = pv.AlternativeCode ?? String.Empty,
                                    DisplayOrder = pv.DisplayOrder ?? 0,
                                    DiscountPercent = (float) pv.DiscountPercent,
                                    DiscountPrice = (float)pv.DiscountPrice,
                                    Price = (float) storePrices.FirstOrDefault(x => x.ProductVariantId == pv.Id).OverridePrice,
                                    PriceCOGS = (float)pv.PriceCOGS,
                                    IsActive = pv.IsActive,
                                    IsMenuDisplay = pv.IsMenuDisplay ?? false
                                }).ToList()
                            }
                        }
                    };
                    categoryItem.Products.Add(productItem);
                }
            }
    
            response.Response.Add(categoryItem);
        }
        return response;
    }
    // public override async Task<GetMenuProductByStoreResponse> GetMenuProductByStore(
    //     GetMenuProductByStoreRequest request, ServerCallContext context)
    // {
    //     var variantIds = request.ListProductVariantIds.ProductVariantId.Select(Guid.Parse).ToList();
    //
    //     var productVariants = await _unitOfWork.GetRepository<ProductVariants>().GetListAsync(
    //         predicate: x => variantIds.Contains(x.Id),
    //         include: x => x.Include(x => x.Product)
    //             .ThenInclude(x => x.Category)
    //     );
    //     // var categories = await _unitOfWork.GetRepository<Categories>().GetListAsync(
    //     //     include: x => x.Include(c => c.Products)
    //     //         .ThenInclude(p => p.ProductVariants.Where(pv => variantIds.Contains(pv.Id)) )
    //     // );
    //     
    //     var groupByCategory = productVariants
    //         .GroupBy(v => v.Product!.Category)
    //         .OrderBy(c => c.Key?.DisplayOrder)
    //         .ToList();
    //     var response = new GetMenuProductByStoreResponse();
    //     foreach (var categoryGroup in groupByCategory)
    //     {
    //         var categoryItem = new CategoryResponse()
    //         {
    //             Id = categoryGroup.Key?.Id.ToString(),
    //             Name = categoryGroup.Key?.Name ?? String.Empty,
    //             DisplayOrder = categoryGroup.Key?.DisplayOrder ?? 0,
    //             Code = categoryGroup.Key?.Code ?? String.Empty,
    //             Description = categoryGroup.Key?.Description ?? String.Empty,
    //         };
    //         foreach (var productGroup in categoryGroup.GroupBy(v => v.Product))
    //         {
    //             var variantsInGroup = productGroup.ToList();
    //             var product = productGroup.Key;
    //
    //             if (variantsInGroup.Count == 1)
    //             {
    //                 var variant = variantsInGroup.First();
    //                 var productItem = new ProductResponse()
    //                 {
    //                     Id = variant.Id.ToString(),
    //                     Code = variant.Code ?? String.Empty,
    //                     Name = variant.Name ?? String.Empty,
    //                     AlternativeCode = variant.AlternativeCode ?? String.Empty,
    //                     ImageUrl = variant.Product!.ProductImages.FirstOrDefault().ImageUrl ?? String.Empty,
    //                     ProductVariants =
    //                     {
    //                     }
    //                 };
    //                 categoryItem.Products.Add(productItem);
    //             }
    //             else
    //             {
    //                 var productItem = new ProductResponse()
    //                 {
    //                     Id = product.Id.ToString(),
    //                     Code = product.Code,
    //                     Name = product.Name,
    //                     AlternativeCode = product.AlternativeCode,
    //                     ImageUrl = product.ProductImages.FirstOrDefault().ImageUrl,
    //                 };
    //                 foreach (var variant in variantsInGroup.OrderBy(v => v.DisplayOrder))
    //                 {
    //                     productItem.ProductVariants.Add(
    //                         new ProductVariant()
    //                         {
    //
    //                             Id = variant.Id.ToString(),
    //                             Code = variant.Code ?? String.Empty,
    //                             Name = variant.Name ?? String.Empty,
    //                             AlternativeCode = variant.AlternativeCode ?? String.Empty,
    //                             DisplayOrder = variant.DisplayOrder ?? 0,
    //                             DiscountPercent = (float)variant.DiscountPercent,
    //                             DiscountPrice = (float)variant.DiscountPrice,
    //                             Price = (float)variant.Price,
    //                             PriceCOGS = (float)variant.PriceCOGS,
    //                             IsActive = variant.IsActive ?? false,
    //                             IsMenuDisplay = variant.IsMenuDisplay ?? false,
    //                         });
    //                 }
    //
    //                 categoryItem.Products.Add(productItem);
    //             }
    //         }
    //
    //         response.Response.Add(categoryItem);
    //     }
    //
    //     return response;
    // }
}
