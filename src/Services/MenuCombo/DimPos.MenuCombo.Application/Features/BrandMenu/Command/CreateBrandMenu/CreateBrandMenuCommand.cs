using DimPos.MenuCombo.Domain.Enums;
using DimPos.MenuCombo.Domain.Models.Common;
using Mediator;

namespace DimPos.MenuCombo.Application.Features.BrandMenu.Command.CreateBrandMenu;

public class CreateBrandMenuCommand : IRequest<ApiResponse>
{
    public string Name { get; set; }
    public string Description { get; set; }
    public EBrandMenuType? Type { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    
}