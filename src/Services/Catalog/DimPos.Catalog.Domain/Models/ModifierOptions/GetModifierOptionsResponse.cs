namespace DimPos.Catalog.Domain.Models.ModifierOptions;

public class GetModifierOptionsResponse
{
    public Guid Id { get; set; }
     public string? Name { get; set; }
     public string? Description { get; set; }
     public bool IsActive { get; set; }
     public decimal? PriceDelta { get; set; }
}