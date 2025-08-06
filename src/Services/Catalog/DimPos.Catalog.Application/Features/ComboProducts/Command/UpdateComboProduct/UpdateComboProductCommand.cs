using DimPos.Catalog.Domain.Models.Common;
using Mediator;

namespace DimPos.Catalog.Application.Features.ComboProducts.Command.UpdateComboProduct;

public class UpdateComboProductCommand : IRequest<ApiResponse>
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int? DisplayOrder { get; set; }
    public bool? IsActive { get; set; }
    public decimal? Price { get; set; }
    public List<UpdateComboProductImages>? ExistComboProductImages { get; set; } = new List<UpdateComboProductImages>();
    public List<UpdateNewComboProductImages>? NewComboProductImages { get; set; } = new List<UpdateNewComboProductImages>();
}
public class UpdateComboProductImages
{
    public Guid Id { get; set; }
    // public string ImageUrl { get; set; }
    public bool IsMainImage { get; set; }
    public string? AltText { get; set; }
}

public class UpdateNewComboProductImages
{
    public bool IsMainImage { get; set; }
    public IFormFile Image { get; set; }
    public string? AltText { get; set; }
}
public class UpdateComboProductRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int? DisplayOrder { get; set; }
    public bool? IsActive { get; set; }
    public decimal? Price { get; set; }
    public List<UpdateComboProductImages>? ExistComboProductImages { get; set; } = new List<UpdateComboProductImages>();
    public List<UpdateNewComboProductImages>? NewComboProductImages { get; set; } = new List<UpdateNewComboProductImages>();
}