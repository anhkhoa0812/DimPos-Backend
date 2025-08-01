using DimPos.Order.Application.Common.Protos;
using DimPos.Order.Infrastructure.Persistence;
using DimPos.Order.Infrastructure.Repositories.Interface;
using Grpc.Core;

namespace DimPos.Order.Application.GrpcServices;

public class OrderGrpcService : Common.Protos.OrderGrpcService.OrderGrpcServiceBase
{
    private readonly IUnitOfWork<OrderContext> _unitOfWork;
    private readonly ILogger _logger;
    
    public OrderGrpcService(IUnitOfWork<OrderContext> unitOfWork, ILogger logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    public override async Task<GetIsNeedToUpdateInventoryByOrderIdResponse> GetIsNeedToUpdateInventoryByOrderId(GetIsNeedToUpdateInventoryByOrderIdRequest request, ServerCallContext context)
    {
        var orderId = Guid.Parse(request.OrderId);
        var storeId = Guid.Parse(request.StoreId);
        
        _logger.Information("BEGIN: GetIsNeedToUpdateInventoryByOrderId for OrderId: {OrderId}, StoreId: {StoreId}", orderId, storeId);
        var order = await _unitOfWork.GetRepository<Domain.Entities.Orders>().SingleOrDefaultAsync(
            predicate: x => x.Id == orderId && x.StoreId == storeId
        );
        if (order == null)
        {
            _logger.Error("Order not found for OrderId: {OrderId}, StoreId: {StoreId}", orderId, storeId);
            throw new RpcException(new Status(StatusCode.NotFound, "Không tìm thấy đơn hàng"));
        }

        return new GetIsNeedToUpdateInventoryByOrderIdResponse()
        {
            OrderId = order.Id.ToString(),
            IsNeedToUpdateInventory = order.IsNeedToUpdateInventory
        };
    }
}