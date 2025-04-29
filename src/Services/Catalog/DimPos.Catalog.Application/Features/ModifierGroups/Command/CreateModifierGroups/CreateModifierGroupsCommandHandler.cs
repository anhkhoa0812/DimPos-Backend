using System.Net;
using DimPos.Catalog.Application.Common.Mapper;
using DimPos.Catalog.Domain.Entities;
using DimPos.Catalog.Domain.Models.Common;
using DimPos.Catalog.Infrastructure.Persistence;
using DimPos.Catalog.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.Catalog.Application.Features.ModifierGroups.Command.CreateModifierGroups;

public class CreateModifierGroupsCommandHandler : IRequestHandler<CreateModifierGroupsCommand, ApiResponse>
{
    private readonly IUnitOfWork<CatalogContext> _unitOfWork;
    private readonly ILogger _logger;

    public CreateModifierGroupsCommandHandler(IUnitOfWork<CatalogContext> unitOfWork, ILogger logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }
    public async ValueTask<ApiResponse> Handle(CreateModifierGroupsCommand request, CancellationToken cancellationToken)
    {
        _logger.Information($"BEGIN: {nameof(CreateModifierGroupsCommandHandler)} - {DateTime.UtcNow}");
        var modifierGroup = ModifierGroupsMapper.ToModifierGroups(request);
        modifierGroup.Id = Guid.NewGuid();
        
        if (request.ModifierOptions != null)
        {
            modifierGroup.ModifierOptions = new List<ModifierOptions>();
            foreach (var option in request.ModifierOptions)
            {
                var modifierOption = new ModifierOptions()
                {
                    Id = Guid.NewGuid(),
                    Name = option.Name,
                    Description = option.Description,
                    Status = option.Status,
                    PriceDelta = option.PriceDelta
                };
                modifierGroup.ModifierOptions.Add(modifierOption);
            }
        }

        await _unitOfWork.GetRepository<Domain.Entities.ModifierGroups>().InsertAsync(modifierGroup);
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        _logger.Information($"END: {nameof(CreateModifierGroupsCommandHandler)} - {DateTime.UtcNow}");
        if (isSuccess)
        {
            return new ApiResponse()
            {
                Status = HttpStatusCode.Created,
                Message = "Tạo nhóm tùy chọn thành công",
            };
        }
        return new ApiResponse()
        {
            Status = HttpStatusCode.InternalServerError,
            Message = "Tạo nhóm tùy chọn thất bại",
        };
    }
}