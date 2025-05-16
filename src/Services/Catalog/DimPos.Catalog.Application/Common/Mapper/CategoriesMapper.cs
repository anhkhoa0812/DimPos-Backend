using DimPos.Catalog.Application.Features.Categories.Command.CreateCategories;
using DimPos.Catalog.Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace DimPos.Catalog.Application.Common.Mapper;

[Mapper]
public static partial class CategoriesMapper
{
    public static partial Categories ToCategories(CreateCategoriesCommand request);
}