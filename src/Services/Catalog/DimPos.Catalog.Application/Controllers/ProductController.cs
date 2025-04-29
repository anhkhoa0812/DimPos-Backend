using DimPos.Catalog.Application.Features.Products.Commands.CreateProducts;
using DimPos.Catalog.Application.Features.Products.Query.GetAllProducts;
using DimPos.Catalog.Domain.Constants;
using DimPos.Catalog.Domain.Models.Common;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace DimPos.Catalog.Application.Controllers;

[ApiController]
[Route(ApiEndPointConstants.Products.ProductsEndpoint)]
public class ProductController : BaseController<ProductController>
{
    private readonly IMediator _mediator;
    public ProductController(ILogger logger, IMediator mediator) : base(logger)
    {
        _mediator = mediator;
    }
    [HttpPost(ApiEndPointConstants.Products.ProductsEndpoint)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ApiResponse> CreateProduct([FromForm] CreateProductsCommand createProductCommand)
    {
        var result = await _mediator.Send(createProductCommand);
        return result;
    }
    [HttpGet(ApiEndPointConstants.Products.ProductsEndpoint)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<ApiResponse> GetAllProducts()
    {
        var command = new GetAllProductsQueries();
        var result = await _mediator.Send(command);
        return result;
    }
}