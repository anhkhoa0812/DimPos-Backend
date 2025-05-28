using DimPos.Catalog.Application.Common.Exceptions;
using DimPos.Catalog.Application.Services.Interface;
using DimPos.Catalog.Domain.Models.Common;
using DimPos.Catalog.Infrastructure.Persistence;
using DimPos.Catalog.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.Catalog.Application.Features.ModifierGroups.Command.UpdateModifierGroups;

public class UpdateModifierGroupsCommandHandler : IRequestHandler<UpdateModifierGroupsCommand, ApiResponse>
{
    private readonly IUnitOfWork<CatalogContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    
    public UpdateModifierGroupsCommandHandler(IUnitOfWork<CatalogContext> unitOfWork, ILogger logger,
        IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(UpdateModifierGroupsCommand request, CancellationToken cancellationToken)
    {
        var modifierGroup = await _unitOfWork.GetRepository<Domain.Entities.ModifierGroups>().SingleOrDefaultAsync(
            predicate: x => x.Id == request.Id && x.BrandId == _claimService.GetBrandId
        );
        if (modifierGroup == null)
        {
            _logger.Error("Modifier group with ID {Id} not found.", request.Id);
            throw new NotFoundException("Không tìm thấy nhóm tùy chỉnh với ID đã cung cấp.");
        }
        modifierGroup.Name = request.UpdateModifierGroupsRequest.Name ?? modifierGroup.Name;
        modifierGroup.Description = request.UpdateModifierGroupsRequest.Description ?? modifierGroup.Description;
        modifierGroup.IsActive = request.UpdateModifierGroupsRequest.IsActive ?? modifierGroup.IsActive;
        modifierGroup.SelectedType = request.UpdateModifierGroupsRequest.SelectedType ?? modifierGroup.SelectedType;
        modifierGroup.DisplayOrder = request.UpdateModifierGroupsRequest.DisplayOrder ?? modifierGroup.DisplayOrder;
        modifierGroup.IsActive = request.UpdateModifierGroupsRequest.IsActive ?? modifierGroup.IsActive;
        _unitOfWork.GetRepository<Domain.Entities.ModifierGroups>().UpdateAsync(modifierGroup);
        
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if(!isSuccess)
            throw new Exception("Cập nhật nhóm tùy chỉnh không thành công.");
        return new ApiResponse()
        {
            Status = 200,
            Message = "Cập nhật nhóm tùy chỉnh thành công.",
            Data = null
        };
    }
}