using DimPos.Store.Application.Services.Interface;
using DimPos.Store.Domain.Entities;
using DimPos.Store.Domain.Models.Common;
using DimPos.Store.Infrastructure.Persistence;
using DimPos.Store.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.Store.Application.Features.TaxRate.Command.UpdateTaxRate;

public class UpdateTaxRateCommandHandler : IRequestHandler<UpdateTaxRateCommand, ApiResponse>
{
    private readonly IUnitOfWork<StoreContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    
    public UpdateTaxRateCommandHandler(IUnitOfWork<StoreContext> unitOfWork, ILogger logger, IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(UpdateTaxRateCommand request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if (brandId == Guid.Empty)
        {
            _logger.Error("Không tìm thấy thông tin thương hiệu trong yêu cầu cập nhật thuế cho cửa hàng {StoreId}", request.StoreId);
            throw new BadHttpRequestException("Không tìm thấy thông tin thương hiệu.");
        }

        var taxRateList = await _unitOfWork.GetRepository<TaxRates>().GetListAsync(
            predicate: x => x.StoreId == request.StoreId && x.BrandId == brandId
        );
        
        if (taxRateList == null || !taxRateList.Any())
        {
            _logger.Error("Không tìm thấy thuế nào cho cửa hàng {StoreId} với thương hiệu {BrandId}", request.StoreId, brandId);
            throw new BadHttpRequestException("Không tìm thấy thuế nào cho cửa hàng này.");
        }
        
        
        var taxRate = taxRateList.FirstOrDefault(x => x.Id.Equals(request.TaxRateId));
        if (taxRate == null)
        {
            _logger.Error("Không tìm thấy thuế với Id {TaxRateId} cho cửa hàng {StoreId}", request.TaxRateId, request.StoreId);
            throw new BadHttpRequestException("Không tìm thấy thuế với Id này.");
        }
        taxRate.Name = request.Name ?? taxRate.Name;
        taxRate.Rate = request.Rate ?? taxRate.Rate;
        if (request.IsActive != null)
        {
            if (request.IsActive.Value)
            {
                if (taxRateList.Any(x => x.IsActive))
                {
                    _logger.Error("Cửa hàng {StoreId} đã có thuế đang hoạt động. Không thể kích hoạt thuế mới", request.StoreId);
                    throw new BadHttpRequestException("Cửa hàng này đã có thuế đang hoạt động.");
                }
            }
            taxRate.IsActive = request.IsActive.Value;
        }

        _unitOfWork.GetRepository<TaxRates>().UpdateAsync(taxRate);
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (!isSuccess)
        {
            _logger.Error("Lỗi khi cập nhật thuế cho cửa hàng {StoreId} với tên {Name}", request.StoreId, request.Name);
            throw new BadHttpRequestException("Đã xảy ra lỗi khi cập nhật thuế.");
        }
        _logger.Information("Cập nhật thuế thành công cho cửa hàng {StoreId} với tên {Name}", request.StoreId, request.Name);
        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Cập nhật thuế thành công.",
            Data = null
        };
    }
}