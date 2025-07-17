using DimPos.Store.Application.Services.Interface;
using DimPos.Store.Domain.Entities;
using DimPos.Store.Domain.Models.Common;
using DimPos.Store.Infrastructure.Persistence;
using DimPos.Store.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.Store.Application.Features.StorePaymentMethodConfig.Command.UpdateStorePaymentConfig;

public class UpdateStorePaymentConfigCommandHandler : IRequestHandler<UpdateStorePaymentConfigCommand, ApiResponse>
{
    private readonly IUnitOfWork<StoreContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    
    public UpdateStorePaymentConfigCommandHandler(IUnitOfWork<StoreContext> unitOfWork, ILogger logger, IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(UpdateStorePaymentConfigCommand request, CancellationToken cancellationToken)
    {
        var storeId = _claimService.GetStoreId ?? Guid.Empty;
        if (storeId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy cửa hàng");
        }

        var storePaymentMethodConfig = await _unitOfWork.GetRepository<StorePaymentMethodConfigs>()
            .SingleOrDefaultAsync(
                predicate: x => x.StoreId == storeId && x.Id == request.StorePaymentMethodConfigId
            );
        if (storePaymentMethodConfig == null) 
        {
            throw new BadHttpRequestException("Cấu hình phương thức thanh toán không tồn tại cho cửa hàng này");
        }
        
        storePaymentMethodConfig.IsActiveByStore = request.IsActiveByStore;
        _unitOfWork.GetRepository<StorePaymentMethodConfigs>().UpdateAsync(storePaymentMethodConfig);
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (!isSuccess)
        {
            throw new BadHttpRequestException("Cập nhật cấu hình phương thức thanh toán không thành công");
        }
        
        return new ApiResponse
        {
            Status = StatusCodes.Status200OK,
            Message = "Cập nhật cấu hình phương thức thanh toán thành công",
            Data = storePaymentMethodConfig.Id
        };
    }
}