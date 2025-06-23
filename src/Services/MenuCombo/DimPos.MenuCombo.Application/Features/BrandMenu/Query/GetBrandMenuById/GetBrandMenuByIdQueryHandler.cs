using DimPos.MenuCombo.Application.Services.Interface;
using DimPos.MenuCombo.Domain.Models.Common;
using DimPos.MenuCombo.Infrastructure.Persistence;
using DimPos.MenuCombo.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.MenuCombo.Application.Features.BrandMenu.Query.GetBrandMenuById;

public class GetBrandMenuByIdQueryHandler : IRequestHandler<GetBrandMenuByIdQuery, ApiResponse>
{
    private readonly IUnitOfWork<MenuComboContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    
    public GetBrandMenuByIdQueryHandler(IUnitOfWork<MenuComboContext> unitOfWork, ILogger logger, IClaimService claimService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _claimService = claimService;
    }
    
    public async ValueTask<ApiResponse> Handle(GetBrandMenuByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.Information("GetBrandMenuByIdQueryHandler.Handle called with request: {@Request}", request);
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if (brandId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy thương hiệu");
        }

        var brandMenuResponse = await _unitOfWork.GetRepository<Domain.Entities.BrandMenu>().SingleOrDefaultAsync(
            predicate: x => x.BrandId == brandId && x.Id == request.BrandMenuId,
            selector: x => new Domain.Models.BrandMenu.BrandMenuResponse
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                IsActiveByBrand = x.IsActiveByBrand,
                Type = x.Type,
                ValidFrom = x.ValidFrom,
                ValidTo = x.ValidTo
            }
        );
        if (brandMenuResponse == null)
        {
            throw new BadHttpRequestException("Không tìm thấy menu của thương hiệu");
        }
        _logger.Information("Brand menu found: {@BrandMenuResponse}", brandMenuResponse);
        return new ApiResponse
        {
            Status = 200,
            Message = "Lấy thông tin menu thành công",
            Data = brandMenuResponse
        };
    }
}