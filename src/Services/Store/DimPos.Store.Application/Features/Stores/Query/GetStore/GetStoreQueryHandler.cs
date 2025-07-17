using DimPos.Store.Application.Services.Interface;
using DimPos.Store.Domain.Models.Common;
using DimPos.Store.Domain.Models.Response;
using DimPos.Store.Infrastructure.Persistence;
using DimPos.Store.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.Store.Application.Features.Stores.Query.GetStore;

public class GetStoreQueryHandler : IRequestHandler<GetStoreQuery, ApiResponse>
{
    private readonly IUnitOfWork<StoreContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    
    public GetStoreQueryHandler(IUnitOfWork<StoreContext> unitOfWork, ILogger logger, IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(GetStoreQuery request, CancellationToken cancellationToken)
    {
        var storeId = _claimService.GetStoreId ?? Guid.Empty;
        if (storeId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy cửa hàng");
        }

        var store = await _unitOfWork.GetRepository<Domain.Entities.Store>().SingleOrDefaultAsync(
            predicate: x => x.Id == storeId
        );
        if (store == null)
        {
            throw new BadHttpRequestException("Cửa hàng không tồn tại");
        }
        var response = new GetStoreResponse()
        {
            Id = store.Id,
            Name = store.Name,
            Code = store.Code,
            Email = store.Email,
            Phone = store.Phone,
            ShortName = store.ShortName,
            Description = store.Description,
            Address = store.Address,
            Latitude = store.Latitude,
            Longitude = store.Longitude,
            Status = store.Status,
            WifiName = store.WifiName,
            WifiPassword = store.WifiPassword,
            Index = store.Index,
            LocalPasscode = store.LocalPasscode,
            ManagerName = store.ManagerName,
            StartingStoreCashLending = store.StartingStoreCashLending,
            Type = store.Type,
            CreatedDate = store.CreatedDate,
            LastModifiedDate = store.LastModifiedDate
        };
        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Lấy thông tin cửa hàng thành công",
            Data = response
        };
    }
}