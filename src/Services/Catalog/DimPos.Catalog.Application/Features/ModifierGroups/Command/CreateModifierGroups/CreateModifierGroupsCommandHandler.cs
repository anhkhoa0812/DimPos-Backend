using System.Net;
using DimPos.Catalog.Application.Common.Mapper;
using DimPos.Catalog.Application.Services.Interface;
using DimPos.Catalog.Domain.Entities;
using DimPos.Catalog.Domain.Models.Common;
using DimPos.Catalog.Infrastructure.Persistence;
using DimPos.Catalog.Infrastructure.Repositories.Interface;
using DimPos.Catalog.Infrastructure.Utils;
using Mediator;

namespace DimPos.Catalog.Application.Features.ModifierGroups.Command.CreateModifierGroups;

public class CreateModifierGroupsCommandHandler : IRequestHandler<CreateModifierGroupsCommand, ApiResponse>
{
    private readonly IUnitOfWork<CatalogContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    
    public CreateModifierGroupsCommandHandler(IUnitOfWork<CatalogContext> unitOfWork, ILogger logger, IClaimService claimService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _claimService = claimService;
    }
    public async ValueTask<ApiResponse> Handle(CreateModifierGroupsCommand request, CancellationToken cancellationToken)
    {
        _logger.Information($"BEGIN: {nameof(CreateModifierGroupsCommandHandler)} - {TimeUtil.GetCurrentSEATime()}");
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if (brandId == Guid.Empty)
            throw new BadHttpRequestException("Không tìm thấy Id của thương hiệu");
        var modifierGroup = ModifierGroupsMapper.ToModifierGroups(request);
        modifierGroup.Id = Guid.CreateVersion7();
        modifierGroup.BrandId = brandId;
        if (request.ModifierOptions != null)
        {
            modifierGroup.ModifierOptions = new List<DimPos.Catalog.Domain.Entities.ModifierOptions>();
            foreach (var option in request.ModifierOptions)
            {
                var modifierOption = new DimPos.Catalog.Domain.Entities.ModifierOptions()
                {
                    Id = Guid.CreateVersion7(),
                    Name = option.Name,
                    Description = option.Description,
                    IsActive = option.IsActive,
                    PriceDelta = option.PriceDelta
                };
                modifierGroup.ModifierOptions.Add(modifierOption);
            }
        }

        await _unitOfWork.GetRepository<Domain.Entities.ModifierGroups>().InsertAsync(modifierGroup);
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        _logger.Information($"END: {nameof(CreateModifierGroupsCommandHandler)} - {TimeUtil.GetCurrentSEATime()}");
        if (isSuccess)
        {
            return new ApiResponse()
            {
                Status =  (int) HttpStatusCode.Created,
                Message = "Tạo nhóm tùy chọn thành công",
            };
        }
        return new ApiResponse()
        {
            Status = (int) HttpStatusCode.InternalServerError,
            Message = "Tạo nhóm tùy chọn thất bại",
        };
    }
}