using DimPos.Catalog.Domain.Constants;
using Microsoft.AspNetCore.Mvc;

namespace DimPos.Catalog.Application.Controllers;

[Route(ApiEndPointConstants.ApiEndpoint)]
[ApiController]
public class BaseController<T> : ControllerBase where T : BaseController<T>
{
    protected ILogger _logger;

    public BaseController(ILogger logger)
    {
        _logger = logger;
    }
}