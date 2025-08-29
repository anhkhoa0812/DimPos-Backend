using DimPos.Store.Application.Services.Interface;
using DimPos.Store.Domain.Entities;
using DimPos.Store.Domain.Models.Common;
using DimPos.Store.Infrastructure.Persistence;
using DimPos.Store.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.Store.Application.Features.TaxRate.Command.CreateTaxRate;

public class CreateTaxRateCommandHandler : IRequestHandler<CreateTaxRateCommand, ApiResponse>
{
    private readonly IUnitOfWork<StoreContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;

    public CreateTaxRateCommandHandler(IUnitOfWork<StoreContext> unitOfWork, ILogger logger, IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    public async ValueTask<ApiResponse> Handle(CreateTaxRateCommand request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if (brandId == Guid.Empty)
        {
            _logger.Error("Không tìm thấy thông tin thương hiệu trong yêu cầu tạo thuế mới cho cửa hàng {StoreId}", request.StoreId);
            throw new BadHttpRequestException("Không tìm thấy thông tin thương hiệu.");
        }

        var existingTaxRates = await _unitOfWork.GetRepository<TaxRates>().GetListAsync(
            predicate: x => x.StoreId == request.StoreId
        );
        
        var taxRate = new TaxRates()
        {
            Id = Guid.CreateVersion7(),
            BrandId = brandId,
            StoreId = request.StoreId,
            Name = request.Name,
            Rate = request.Rate,
            IsActive = !existingTaxRates.Any()
        };
        await _unitOfWork.GetRepository<TaxRates>().InsertAsync(taxRate);
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (!isSuccess)
        {
            _logger.Error("Lỗi khi tạo thuế mới cho cửa hàng {StoreId} với tên {Name}", request.StoreId, request.Name);
            throw new BadHttpRequestException("Đã xảy ra lỗi khi tạo thuế mới.");
        }
        _logger.Information("Tạo thuế mới thành công cho cửa hàng {StoreId} với tên {Name}", request.StoreId, request.Name);
        return new ApiResponse()
        {
            Status = StatusCodes.Status201Created,
            Message = "Tạo thuế mới thành công.",
            Data = null
        };
    }
}