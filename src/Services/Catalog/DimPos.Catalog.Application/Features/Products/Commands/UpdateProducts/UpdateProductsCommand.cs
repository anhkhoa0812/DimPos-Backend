using DimPos.Catalog.Domain.Enums;
using DimPos.Catalog.Domain.Models.Common;
using Mediator;

namespace DimPos.Catalog.Application.Features.Products.Commands.UpdateProducts;

public class UpdateProductsCommand : IRequest<ApiResponse>
{
    public Guid ProductId { get; set; }
    public UpdateProductsRequest UpdateProducts { get; set; } = new();
}

public class UpdateProductsRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? AlternativeCode { get; set; }
    public EProductStatus? Status { get; set; }
    public bool? IsAvailable { get; set; }
    public int? DisplayOrder { get; set; }
    public bool? IsMenuDisplay { get; set; }
    public bool? IsMostOrdered { get; set; }
    public EProductSaleType? SaleType { get; set; }
    public string? Note { get; set; }
    public List<UpdateProductImages>? ExistProductImages { get; set; } = new List<UpdateProductImages>();
    public List<UpdateNewProductImages>? NewProductImages { get; set; } = new List<UpdateNewProductImages>();
}

public class UpdateProductImages
{
    public Guid Id { get; set; }
    // public string ImageUrl { get; set; }
    public bool IsMainImage { get; set; }
    public string? AltText { get; set; }
}

public class UpdateNewProductImages
{
    public bool IsMainImage { get; set; }
    public IFormFile Image { get; set; }
    public string? AltText { get; set; }
}