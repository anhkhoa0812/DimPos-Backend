using DimPos.Brand.Application.Features.Brands.Command;
using DimPos.Brand.Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace DimPos.Brand.Application.Common.Mapper;

[Mapper]
public static partial class BrandMapper
{
    public static partial Brands ToBrands(CreateBrandCommand createBrandCommand);
}