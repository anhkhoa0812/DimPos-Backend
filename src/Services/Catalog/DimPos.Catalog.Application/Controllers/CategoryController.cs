using DimPos.Catalog.Application.Features.Categories;
using DimPos.Catalog.Domain.Constants;
using DimPos.Catalog.Domain.Models.Common;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace DimPos.Catalog.Application.Controllers;

[ApiController]
[Route(ApiEndPointConstants.Categories.CategoriesEndpoint)]
public class CategoryController : BaseController<CategoryController>
{
    private readonly IMediator _mediator;
    
    public CategoryController(ILogger logger, IMediator mediator) : base(logger)
    {
        _mediator = mediator;
    }
    [HttpPost(ApiEndPointConstants.Categories.CategoriesEndpoint)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ApiResponse> CreateCategory([FromBody] CreateCategoriesCommand command)
    {
        var response = await _mediator.Send(command);
        return response;
    }
}