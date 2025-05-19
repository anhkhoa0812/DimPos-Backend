using DimPos.Brand.Domain.Enums;

namespace DimPos.Brand.Domain.Models.Brand;

public class GetBrandDetailResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? PictureUrl { get; set; }
    public DateTime? ArchivedAt { get; set; }
    public EBrandStatus Status { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; } 
}