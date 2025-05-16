using DimPos.MenuCombo.Application.Common.Mapper;
using DimPos.MenuCombo.Application.Services.Interface;
using DimPos.MenuCombo.Domain.Models.Common;
using DimPos.MenuCombo.Infrastructure.Persistence;
using DimPos.MenuCombo.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.MenuCombo.Application.Features.BrandMenu.Command.CreateBrandMenu;

public class CreateBrandMenuCommandHandler: IRequestHandler<CreateBrandMenuCommand, ApiResponse>
{
    private readonly IUnitOfWork<MenuComboContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    public CreateBrandMenuCommandHandler(IUnitOfWork<MenuComboContext> unitOfWork, ILogger logger, IClaimService claimService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _claimService = claimService;
    }
    public async ValueTask<ApiResponse> Handle(CreateBrandMenuCommand request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if (brandId == Guid.Empty)
        {
            return new ApiResponse
            {
                Status = 404,
                Message = "Id thương hiệu không hợp lệ",
            };
        }

        if (request.ValidFrom == null && request.ValidTo == null && (request.ValidTo <= request.ValidFrom ))
        { 
            return new ApiResponse
            {
                Status = 400,
                Message = "Thời gian không hợp lệ",
            };
        }

        var brandMenu = BrandMenuMapper.ToBrandMenu(request);
        brandMenu.Id = Guid.CreateVersion7();
        brandMenu.BrandId = brandId;

        await _unitOfWork.GetRepository<Domain.Entities.BrandMenu>().InsertAsync(brandMenu);
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (isSuccess)
        {
            return new ApiResponse
            {
                Status = 200,
                Message = "Thêm mới menu thành công",
                Data = null
            };
        }
        return new ApiResponse
        {
            Status = 500,
            Message = "Thêm mới menu không thành công",
            Data = null
        };
    }
}