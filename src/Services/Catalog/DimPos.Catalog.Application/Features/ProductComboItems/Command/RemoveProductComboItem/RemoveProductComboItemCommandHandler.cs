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

        var productComboItem =  await _unitOfWork.GetRepository<Domain.Entities.ProductComboItems>()
            .SingleOrDefaultAsync(
                predicate: x => x.Id == request.ProductComboItemId && x.Product.BrandId == brandId,
                include: x => x.Include(x => x.Product)
                    .ThenInclude(x => x.ProductComboItems)
            );
        if (productComboItem == null)
        {
            throw new BadHttpRequestException("Không tìm thấy sản phẩm combo item");
        }
        if (productComboItem.Product.ProductComboItems != null && productComboItem.Product.ProductComboItems.Count <= 2)
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