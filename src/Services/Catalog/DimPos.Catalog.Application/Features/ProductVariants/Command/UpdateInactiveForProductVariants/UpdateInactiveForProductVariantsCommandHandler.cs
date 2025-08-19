using DimPos.Catalog.Application.Services.Interface;
using DimPos.Catalog.Domain.Enums;
using DimPos.Catalog.Domain.Models.Common;
using DimPos.Catalog.Infrastructure.Persistence;
using DimPos.Catalog.Infrastructure.Repositories.Interface;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace DimPos.Catalog.Application.Features.ProductVariants.Command.UpdateInactiveForProductVariants;

public class UpdateInactiveForProductVariantsCommandHandler : IRequestHandler<UpdateInactiveForProductVariantsCommand, ApiResponse>
{
    private readonly IUnitOfWork<CatalogContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    
    public UpdateInactiveForProductVariantsCommandHandler(IUnitOfWork<CatalogContext> unitOfWork, ILogger logger, IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(UpdateInactiveForProductVariantsCommand request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if (brandId == Guid.Empty) 
            throw new BadHttpRequestException("Không tìm thấy thông tin thương hiệu trong yêu cầu.");

        var product = await _unitOfWork.GetRepository<Domain.Entities.Products>().SingleOrDefaultAsync(
            predicate: x => x.Id == request.ProductId 
                            && x.BrandId == brandId 
                            && x.Type == EProductType.CustomerOrder 
                            && !x.IsCombo && !x.IsExtra,
            include: x => x.Include(x => x.ProductVariants)
                .ThenInclude(x => x.ProductComboItems)
                .ThenInclude(x => x.Product)
                .ThenInclude(x => x.ProductVariants)
        );
        if (product == null)
        {
            throw new BadHttpRequestException("Không tìm thấy sản phẩm với Id đã cho.");
        }
        if(product.ProductVariants.Any(pv => pv.ProductComboItems.Any(x => x.Product.ProductVariants.Any(pv => pv.IsActive))))
        {
            throw new BadHttpRequestException("Không thể cập nhật trạng thái không hoạt động cho các biến thể sản phẩm, vì có biến thể sản phẩm đang được sử dụng trong combo.");
        }
        if (product.ProductVariants.Any(x => x.IsActive))
        {
            foreach (var productVariant in product.ProductVariants)
            {
                productVariant.IsActive = false;
            }
        
            _unitOfWork.GetRepository<Domain.Entities.Products>().UpdateAsync(product);
            var isSuccess = await _unitOfWork.CommitAsync() > 0;
            if (!isSuccess)
            {
                return new ApiResponse
                {
                    Status = StatusCodes.Status200OK,
                    Message = "Không thể cập nhật trạng thái không hoạt động cho các biến thể sản phẩm.",
                    Data = null
                };
            }
        }

        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Cập nhật trạng thái không hoạt động cho các biến thể sản phẩm thành công.",
            Data = product.Id
        };
    }
}