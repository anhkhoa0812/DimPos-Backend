using DimPos.Catalog.Domain.Models.Common;
using Mediator;

namespace DimPos.Catalog.Application.Features.InternalProducts.Command.UpdateInternalProduct;

public class UpdateInternalProductCommand : IRequest<ApiResponse>
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Code { get; set; }
    public string? Description { get; set; }
    public string? Sku { get; set; }
    public int? DisplayOrder { get; set; }
    public bool? IsActive { get; set; }
    public decimal? Price { get; set; }
    public List<UpdateInternalProductImages>? ExistInternalProductImages { get; set; } = new List<UpdateInternalProductImages>();
    public List<UpdateNewInternalProductImages>? NewInternalProductImages { get; set; } = new List<UpdateNewInternalProductImages>();
}
public class UpdateInternalProductImages
{
    public Guid Id { get; set; }
    // public string ImageUrl { get; set; }
    public bool IsMainImage { get; set; }
    public string? AltText { get; set; }
}

public class UpdateNewInternalProductImages
{
    public bool IsMainImage { get; set; }
    public IFormFile Image { get; set; }
    public string? AltText { get; set; }
}

public class UpdateInternalProductRequest
{
    public string? Name { get; set; }
    public string? Code { get; set; }
    public string? Description { get; set; }
    public string? Sku { get; set; }
    public int? DisplayOrder { get; set; }
    public bool? IsActive { get; set; }
    public decimal? Price { get; set; }
    public List<UpdateInternalProductImages>? ExistInternalProductImages { get; set; } = new List<UpdateInternalProductImages>();
    public List<UpdateNewInternalProductImages>? NewInternalProductImages { get; set; } = new List<UpdateNewInternalProductImages>();
}