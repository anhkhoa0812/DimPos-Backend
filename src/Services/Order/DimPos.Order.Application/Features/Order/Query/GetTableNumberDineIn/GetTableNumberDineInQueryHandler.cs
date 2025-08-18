using DimPos.Order.Application.Services.Interface;
using DimPos.Order.Domain.Entities;
using DimPos.Order.Domain.Enums;
using DimPos.Order.Domain.Models.Common;
using DimPos.Order.Domain.Models.Response;
using DimPos.Order.Infrastructure.Persistence;
using DimPos.Order.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.Order.Application.Features.Order.Query.GetTableNumberDineIn;

public class GetTableNumberDineInQueryHandler : IRequestHandler<GetTableNumberDineInQuery, ApiResponse>
{
    private readonly IUnitOfWork<OrderContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    
    public GetTableNumberDineInQueryHandler(IUnitOfWork<OrderContext> unitOfWork, ILogger logger, IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(GetTableNumberDineInQuery request, CancellationToken cancellationToken)
    {
        var storeId = _claimService.GetStoreId ?? Guid.Empty;
        if (storeId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy Id cửa hàng");
        }

        var orders = await _unitOfWork.GetRepository<Orders>().GetListAsync(
            predicate: x => x.FinancialShiftId == request.FinancialShiftId &&
                 x.StoreId == storeId &&
                 x.Type == EOrderType.DineIn && x.TableNumberDineIn != null &&
                 (x.Status == EOrderStatus.PendingPayment || x.Status == EOrderStatus.Confirmed)
        );
        var response = new GetTableNumberDineInQueryResponse()
        {
            TakedTableNumber = orders.Select(x => x.TableNumberDineIn.Value).ToList()
        };
        return new ApiResponse
        {
            Status = StatusCodes.Status200OK,
            Message = "Lấy danh sách bàn thành công",
            Data = response
        };
    }
}