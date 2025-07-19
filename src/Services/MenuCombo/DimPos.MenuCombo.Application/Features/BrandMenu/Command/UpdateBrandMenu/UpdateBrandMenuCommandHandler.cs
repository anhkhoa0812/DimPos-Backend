using DimPos.MenuCombo.Application.Services.Interface;
using DimPos.MenuCombo.Domain.Models.Common;
using DimPos.MenuCombo.Infrastructure.Persistence;
using DimPos.MenuCombo.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.MenuCombo.Application.Features.BrandMenu.Command.UpdateBrandMenu;

public class UpdateBrandMenuCommandHandler : IRequestHandler<UpdateBrandMenuCommand, ApiResponse>
{
    private readonly IUnitOfWork<MenuComboContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    
    public UpdateBrandMenuCommandHandler(IUnitOfWork<MenuComboContext> unitOfWork, ILogger logger, IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(UpdateBrandMenuCommand request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if (brandId == Guid.Empty)
        {
            throw new BadHttpRequestException("Id thương hiệu không hợp lệ");
        }

        var brandMenu = await _unitOfWork.GetRepository<Domain.Entities.BrandMenu>().SingleOrDefaultAsync(
            predicate: x => x.BrandId == brandId && x.Id == request.BrandMenuId
        );
        if (brandMenu == null)
        {
            throw new BadHttpRequestException("Không tìm thấy Brand Menu");
        }
        
        brandMenu.Name = request.Name ?? brandMenu.Name;
        brandMenu.Description = request.Description ?? brandMenu.Description;
        brandMenu.Type = request.Type ?? brandMenu.Type;
        
        _unitOfWork.GetRepository<Domain.Entities.BrandMenu>().UpdateAsync(brandMenu);
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (!isSuccess)
        {
            throw new Exception("Cập nhật Brand Menu không thành công");
        }
        return new ApiResponse
        {
            Status = 200,
            Message = "Cập nhật Brand Menu thành công",
            Data = brandMenu.Id
        };
    }
}