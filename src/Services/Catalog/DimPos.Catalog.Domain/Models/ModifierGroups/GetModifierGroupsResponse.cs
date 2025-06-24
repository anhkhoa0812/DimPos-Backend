using DimPos.Catalog.Domain.Entities;
using DimPos.Catalog.Domain.Enums;

namespace DimPos.Catalog.Domain.Models.ModifierGroups;

public class GetModifierGroupsResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ESelectedTypeModifier SelectedType { get; set; }
    public int? DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public List<ModifierOptionsResponse>? ModifierOptions { get; set; } = new();
}

public class ModifierOptionsResponse
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public decimal? PriceDelta { get; set; }
}