using DimPos.Catalog.Application.Common.Exceptions;
using DimPos.Catalog.Application.Services.Interface;
using DimPos.Catalog.Domain.Models.Common;
using DimPos.Catalog.Domain.Models.ModifierGroups;
using DimPos.Catalog.Domain.Models.ModifierOptions;
using DimPos.Catalog.Infrastructure.Persistence;
using DimPos.Catalog.Infrastructure.Repositories.Interface;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace DimPos.Catalog.Application.Features.ModifierOptions.Query.GetModifierOptionsByModifierGroup;

public class GetModifierOptionsByModifierGroupQueryHandler : IRequestHandler<GetModifierOptionsByModifierGroupQuery, ApiResponse>
{
    private readonly IUnitOfWork<CatalogContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService; 
    
    public GetModifierOptionsByModifierGroupQueryHandler(IUnitOfWork<CatalogContext> unitOfWork, ILogger logger,
        IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    public async ValueTask<ApiResponse> Handle(GetModifierOptionsByModifierGroupQuery request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if(brandId == Guid.Empty)
            throw new BadHttpRequestException("Không tìm thấy thông tin thương hiệu trong yêu cầu.");
        _logger.Information("BEGIN: GetModifierOptionsByModifierGroupQueryHandler.Handle - ModifierGroupId: {ModifierGroupId}", request.ModifierGroupsId);
        var modifierOptions = await _unitOfWork.GetRepository<Domain.Entities.ModifierOptions>().GetPagingListAsync(
            selector: x => new GetModifierOptionsResponse()
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                IsActive = x.IsActive,
            },
            page: request.Page,
            size: request.Size,
            sortBy: request.SortBy,
            isAsc: request.IsAsc,
            predicate: x => x.ModifierGroupId == request.ModifierGroupsId 
                            && x.ModifierGroup.BrandId == brandId
                            && (request.Name == null || x.ModifierGroup.Name.Contains(request.Name))
                            && (request.IsActive == null || x.IsActive == request.IsActive)
        );
        if (modifierOptions == null)
        {
            _logger.Error("Modifier option with ID not found");
            throw new NotFoundException("Không tìm thấy tùy chọn tùy chỉnh với ID đã cung cấp.");
        }
        _logger.Information("END: GetModifierOptionsByModifierGroupQueryHandler.Handle - ModifierGroupId: {ModifierGroupId}", request.ModifierGroupsId);
        
        return new ApiResponse
        {
            Status = 200,
            Message = "Lấy dữ liệu thành công",
            Data = modifierOptions
        };
    }
}