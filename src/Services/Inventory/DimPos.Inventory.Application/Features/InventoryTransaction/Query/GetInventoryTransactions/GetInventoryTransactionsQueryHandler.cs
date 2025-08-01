using DimPos.Inventory.Application.Services.Interface;
using DimPos.Inventory.Domain.Entities;
using DimPos.Inventory.Domain.Models.Common;
using DimPos.Inventory.Domain.Models.Response;
using DimPos.Inventory.Infrastructure.Persistence;
using DimPos.Inventory.Infrastructure.Repositories.Interface;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace DimPos.Inventory.Application.Features.InventoryTransaction.Query.GetInventoryTransactions;

public class GetInventoryTransactionsQueryHandler : IRequestHandler<GetInventoryTransactionsQuery, ApiResponse>
{
    private readonly IUnitOfWork<InventoryContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;

    public GetInventoryTransactionsQueryHandler(IUnitOfWork<InventoryContext> unitOfWork, ILogger logger,
        IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(GetInventoryTransactionsQuery request, CancellationToken cancellationToken)
    {
        var storeId = _claimService.GetStoreId ?? Guid.Empty;
        if (storeId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy thông tin cửa hàng");
        }

        var inventoryTransactions = await _unitOfWork.GetRepository<InventoryTransactions>().GetPagingListAsync(
            selector: x => new GetInventoryTransactionsResponse()
            {
                Id = x.Id,
                Type = x.Type,
                QuantityChange = x.QuantityChange,
                Note = x.Note,
                ReasonManualAdjustment = x.ReasonManualAdjustment,
                RelatedStorePurchaseOrderItemId = x.RelatedStorePurchaseOrderItemId,
                RelatedOrderId = x.RelatedOrderId,
                AccountId = x.AccountId,
                CreatedDate = x.CreatedDate,
                LastModifiedDate = x.LastModifiedDate
            },
            predicate: x => x.InventoryStockId == request.InventoryStockId 
                            && x.InventoryStock.StoreId == storeId
                            && (request.FromDate == null || x.CreatedDate >= request.FromDate)
                            && (request.ToDate == null || x.CreatedDate <= request.ToDate),
            page: request.Page,
            size: request.Size,
            sortBy: request.SortBy ?? "CreatedDate",
            isAsc: request.IsAsc
        );
        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Lấy danh sách giao dịch kho thành công",
            Data = inventoryTransactions
        };
    }
}