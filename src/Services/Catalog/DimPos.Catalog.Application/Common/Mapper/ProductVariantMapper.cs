using DimPos.Catalog.Application.Features.Products.Commands.CreateProducts;
using DimPos.Catalog.Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace DimPos.Catalog.Application.Common.Mapper;

[Mapper]
public static partial class ProductVariantMapper
{
    public static partial ProductVariants ToPoProductVariants(CreateProductVariant createProductVariant);
}