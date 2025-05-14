namespace DimPos.MenuCombo.Domain.Models.Stores;

public record StoreByBrandResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Description { get; set; }
    public string Address { get; set; }
    public string? Latitude { get; set; }
    public string? Longitude { get; set; }
    public bool IsSelected { get; set; }
}