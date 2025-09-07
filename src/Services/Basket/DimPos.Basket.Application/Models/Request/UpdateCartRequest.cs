using DimPos.Basket.Application.Enums;

namespace DimPos.Basket.Application.Models.Request;

public class UpdateCartRequest
{
    public EServiceMethod? ServiceMethod { get; set; }
    public int? TakeNumberDineIn { get; set; }
    public string? CustomerNameSnapshot { get; set; }
}

