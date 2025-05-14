using DimPos.MenuCombo.Application.Features.BrandMenu.Command.CreateBrandMenu;
using DimPos.MenuCombo.Domain.Entities;
using DimPos.MenuCombo.Domain.Models.BrandMenu;
using DimPos.MenuCombo.Infrastructure.Paginate.Interface;
using Riok.Mapperly.Abstractions;

namespace DimPos.MenuCombo.Application.Common.Mapper;
[Mapper]
public static partial class BrandMenuMapper
{
    public static partial BrandMenu ToBrandMenu(CreateBrandMenuCommand createBrandMenuCommand);
    
    public static partial IList<BrandMenuResponse> ToBrandMenuResponse(IList<BrandMenu> brandMenu);
}