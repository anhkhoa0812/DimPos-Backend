using DimPos.MenuCombo.Application.Features.BrandMenu.Command.CreateBrandMenu;
using DimPos.MenuCombo.Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace DimPos.MenuCombo.Application.Common.Mapper;
[Mapper]
public static partial class BrandMenuMapper
{
    public static partial BrandMenu ToBrandMenu(CreateBrandMenuCommand createBrandMenuCommand);
}