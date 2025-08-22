using DimPos.Store.Domain.Enums;

namespace DimPos.Store.Domain.Models.Response;

public class GetStorePaymentMethodConfigForPosResponse
{
    public Guid Id { get; set; }
    public Guid SystemPaymentMethodId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public EPaymentMethod PaymentMethod { get; set; }
}