using DimPos.Payment.Domain.Enums;

namespace DimPos.Payment.Domain.Models.SystemPaymentMethod;

public class GetSystemPaymentMethodByIdResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public ESystemPaymentMethod Type { get; set; }
    public string? LogoUrl { get; set; }
    public string? Description { get; set; }
    public bool IsGloballyActive { get; set; }
    public string ConfigurationSchema { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
}