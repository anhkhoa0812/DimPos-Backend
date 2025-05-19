using DimPos.Brand.Application.Features.Brands.Command;
using DimPos.Brand.Domain.Entities;
using DimPos.Brand.Domain.Models.Brand;
using Riok.Mapperly.Abstractions;

namespace DimPos.Brand.Application.Common.Mapper;

[Mapper]
public static partial class BrandMapper
{
    public static partial Brands ToBrands(CreateBrandCommand createBrandCommand);
    public static partial GetBrandDetailResponse ToGetBrandDetailResponse(Brands brands);
}