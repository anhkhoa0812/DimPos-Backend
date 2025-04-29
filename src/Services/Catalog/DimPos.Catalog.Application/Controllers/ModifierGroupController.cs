using DimPos.Catalog.Application.Features.ModifierGroups.Command.CreateModifierGroups;
using DimPos.Catalog.Domain.Constants;
using DimPos.Catalog.Domain.Models.Common;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace DimPos.Catalog.Application.Controllers;

[ApiController]
[Route(ApiEndPointConstants.ModifierGroups.ModifierGroupsEndpoint)]
public class ModifierGroupController :BaseController<ModifierGroupController>
{
    private readonly IMediator _mediator;
    public ModifierGroupController(IMediator mediator, ILogger logger) : base(logger)
    {
        _mediator = mediator;
    }
    
    [HttpPost(ApiEndPointConstants.ModifierGroups.ModifierGroupsEndpoint)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ApiResponse> CreateModifierGroup([FromBody] CreateModifierGroupsCommand command)
    {
        var response = await _mediator.Send(command);
        return response;
    }
    
}