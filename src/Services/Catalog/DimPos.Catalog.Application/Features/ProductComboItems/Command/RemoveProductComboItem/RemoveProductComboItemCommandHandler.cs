using DimPos.Catalog.Application.Services.Interface;
using DimPos.Catalog.Domain.Models.Common;
using DimPos.Catalog.Infrastructure.Persistence;
using DimPos.Catalog.Infrastructure.Repositories.Interface;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace DimPos.Catalog.Application.Features.ProductComboItems.Command.RemoveProductComboItem;

public class RemoveProductComboItemCommandHandler : IRequestHandler<RemoveProductComboItemCommand, ApiResponse>
{
    private readonly IUnitOfWork<CatalogContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    
    public RemoveProductComboItemCommandHandler(IUnitOfWork<CatalogContext> unitOfWork, ILogger logger, IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(RemoveProductComboItemCommand request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if (brandId == Guid.Empty)
            throw new BadHttpRequestException("Không tìm thấy Id của thương hiệu");

        var productVariant = await _unitOfWork.GetRepository<Domain.Entities.ProductVariants>().SingleOrDefaultAsync(
            predicate: x => x.Id == request.ProductVariantId &&
                            x.Product.BrandId == brandId &&
                            x.Product.IsCombo,
            include: x => x.Include(x => x.Product)
                .ThenInclude(x => x.ProductComboItems)
        );
        if (productVariant == null)
        {
            throw new BadHttpRequestException("Không tìm thấy sản phẩm combo");
        }
        
        var productComboItem = productVariant.Product.ProductComboItems?
            .FirstOrDefault(x => x.Id == request.ProductComboItemId);
        if (productComboItem == null)
        {
            throw new BadHttpRequestException("Không tìm thấy sản phẩm combo item");
        }
        if(productVariant.Product.ProductComboItems != null && productVariant.Product.ProductComboItems.Count(x => x != productComboItem) <= 2)
        {
            throw new BadHttpRequestException("Không thể xóa sản phẩm combo item, phải có ít nhất 2 sản phẩm trong combo");
        }
        
        _unitOfWork.GetRepository<Domain.Entities.ProductComboItems>().DeleteAsync(productComboItem);
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (!isSuccess)
        {
            throw new Exception("Xóa sản phẩm combo item không thành công");
        }
        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Xóa sản phẩm combo item thành công",
            Data = productComboItem.Id
        };
    }
}