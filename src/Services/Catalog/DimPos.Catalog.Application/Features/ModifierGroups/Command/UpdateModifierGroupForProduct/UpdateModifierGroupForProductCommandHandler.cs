using DimPos.Catalog.Application.Services.Interface;
using DimPos.Catalog.Domain.Entities;
using DimPos.Catalog.Domain.Models.Common;
using DimPos.Catalog.Infrastructure.Persistence;
using DimPos.Catalog.Infrastructure.Repositories.Interface;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace DimPos.Catalog.Application.Features.ModifierGroups.Command.UpdateModifierGroupForProduct;

public class UpdateModifierGroupForProductCommandHandler : IRequestHandler<UpdateModifierGroupForProductCommand, ApiResponse>
{
    private readonly IUnitOfWork<CatalogContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    
    public UpdateModifierGroupForProductCommandHandler(IUnitOfWork<CatalogContext> unitOfWork, ILogger logger, IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(UpdateModifierGroupForProductCommand request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if (brandId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy Id của thương hiệu");
        }

        var product = await _unitOfWork.GetRepository<Domain.Entities.Products>().SingleOrDefaultAsync(
            predicate: x => x.Id == request.ProductId && x.BrandId == brandId,
            include: x => x.Include(x => x.ProductModifierGroups)
                .ThenInclude(x => x.ModifierGroup)
        );
        if (product == null)
        {
            throw new BadHttpRequestException("Không tìm thấy sản phẩm với ID đã cung cấp.");
        }

        var existingProductModifierGroup = product.ProductModifierGroups?.Select(x => x.ModifierGroupId)
            .ToList();
        
        var newModifierGroupIds = existingProductModifierGroup != null ?
            request.ModifierGroupIds
                .Where(x => !existingProductModifierGroup.Contains(x))
                .ToList() : request.ModifierGroupIds;
        var removedModifierGroupIds = existingProductModifierGroup != null ?
            existingProductModifierGroup
                .Where(x => !request.ModifierGroupIds.Contains(x))
                .ToList() : new List<Guid>();

        if (newModifierGroupIds.Any())
        {
            foreach (var newModifierGroupId in newModifierGroupIds)
            {
                var newProductModifierGroup = new ProductModifierGroups()
                {
                    Id = Guid.CreateVersion7(),
                    ModifierGroupId = newModifierGroupId,
                    ProductId = product.Id
                };
                await _unitOfWork.GetRepository<ProductModifierGroups>().InsertAsync(newProductModifierGroup);
            }
        }

        if (removedModifierGroupIds.Any())
        {
            foreach (var removedModifierGroupId in removedModifierGroupIds)
            {
                var productModiferGroup = product.ProductModifierGroups?.First(x => x.ModifierGroupId == removedModifierGroupId);
                
                if (productModiferGroup != null)
                {
                    _unitOfWork.GetRepository<ProductModifierGroups>().DeleteAsync(productModiferGroup);
                }
            }
        }
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (!isSuccess)
        {
            throw new Exception("Cập nhật nhóm tùy chỉnh cho sản phẩm không thành công.");
        }
        
        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Cập nhật nhóm tùy chỉnh cho sản phẩm thành công.",
            Data = product.Id
        };
    }
}