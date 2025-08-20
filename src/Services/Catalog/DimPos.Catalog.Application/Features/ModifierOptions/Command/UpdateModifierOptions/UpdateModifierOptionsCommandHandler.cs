using DimPos.Catalog.Application.Common.Exceptions;
using DimPos.Catalog.Application.Services.Interface;
using DimPos.Catalog.Domain.Models.Common;
using DimPos.Catalog.Infrastructure.Persistence;
using DimPos.Catalog.Infrastructure.Repositories.Interface;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace DimPos.Catalog.Application.Features.ModifierOptions.Command.UpdateModifierOptions;

public class UpdateModifierOptionsCommandHandler : IRequestHandler<UpdateModifierOptionsCommand, ApiResponse>
{
    private readonly IUnitOfWork<CatalogContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;

    public UpdateModifierOptionsCommandHandler(IUnitOfWork<CatalogContext> unitOfWork, ILogger logger,
        IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    } 
    
    public async ValueTask<ApiResponse> Handle(UpdateModifierOptionsCommand request, CancellationToken cancellationToken)
    {
       var modifierOptions = await _unitOfWork.GetRepository<Domain.Entities.ModifierOptions>().SingleOrDefaultAsync(
            predicate: x => x.Id == request.Id && x.ModifierGroup.BrandId == _claimService.GetBrandId,
            include: x => x.Include(y => y.ModifierGroup)
        );
        if (modifierOptions == null)
        {
            _logger.Error("Modifier option with ID {Id} not found.", request.Id);
            throw new NotFoundException("Không tìm thấy tùy chọn tùy chỉnh với ID đã cung cấp.");
        }
        modifierOptions.Name = request.UpdateModifierOptions.Name ?? modifierOptions.Name;
        modifierOptions.Description = request.UpdateModifierOptions.Description ?? modifierOptions.Description;
        modifierOptions.IsActive = request.UpdateModifierOptions.IsActive ?? modifierOptions.IsActive;

        _unitOfWork.GetRepository<Domain.Entities.ModifierOptions>().UpdateAsync(modifierOptions);
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (!isSuccess)
            throw new Exception("Cập nhật tùy chọn tùy chỉnh không thành công.");
        return new ApiResponse()
        {
            Status = 200,
            Message = "Cập nhật tùy chọn tùy chỉnh thành công.",
            Data = null
        };
    }
}