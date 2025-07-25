using DimPos.Catalog.Application.Services.Interface;
using DimPos.Catalog.Domain.Models.Common;
using DimPos.Catalog.Infrastructure.Persistence;
using DimPos.Catalog.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.Catalog.Application.Features.ProductComboItems.Command.UpdateProductComboItem;

public class UpdateProductComboItemCommandHandler : IRequestHandler<UpdateProductComboItemCommand, ApiResponse>
{
    private readonly IUnitOfWork<CatalogContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    
    public UpdateProductComboItemCommandHandler(IUnitOfWork<CatalogContext> unitOfWork, ILogger logger, IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(UpdateProductComboItemCommand request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if(brandId == Guid.Empty) 
            throw new BadHttpRequestException("Không tìm thấy id của thương hiệu");

        var productComboItem = await _unitOfWork.GetRepository<Domain.Entities.ProductComboItems>()
            .SingleOrDefaultAsync(
                predicate: x => x.Id == request.ProductComboItemId 
                && x.Product.BrandId == brandId
            );
        if (productComboItem == null)
        {
            throw new BadHttpRequestException("Không tìm thấy sản phẩm combo item");
        }
        
        productComboItem.Quantity = request.Quantity ?? productComboItem.Quantity;
        productComboItem.DisplayOrder = request.DisplayOrder ?? productComboItem.DisplayOrder;
        
        _unitOfWork.GetRepository<Domain.Entities.ProductComboItems>().UpdateAsync(productComboItem);
        
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (!isSuccess)
        {
            throw new Exception("Cập nhật sản phẩm combo item không thành công");
        }

        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Cập nhật sản phẩm combo item thành công",
            Data = productComboItem.Id
        };
    }
}