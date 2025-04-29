using DimPos.Catalog.Application.Features.Products.Commands.CreateProducts;
using DimPos.Catalog.Domain.Constants;
using DimPos.Catalog.Domain.Entities;
using DimPos.Catalog.Domain.Models.Product;
using Riok.Mapperly.Abstractions;

namespace DimPos.Catalog.Application.Common.Mapper;

[Mapper]
public static partial class ProductMapper
{
    public static partial Products ToProducts(CreateProductsCommand command);
    public static partial ProductResponse ToProductResponse(Products product);
    public static partial List<ProductResponse> ToProductResponses(List<Products> product);
}