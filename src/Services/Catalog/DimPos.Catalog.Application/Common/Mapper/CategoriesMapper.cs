using DimPos.Catalog.Application.Features.Categories;
using DimPos.Catalog.Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace DimPos.Catalog.Application.Common.Mapper;

[Mapper]
public static partial class CategoriesMapper
{
    public static partial Categories ToCategories(CreateCategoriesCommand request);
}