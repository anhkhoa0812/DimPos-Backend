using DimPos.Store.Application.Services.Interface;
using DimPos.Store.Domain.Models.Common;
using DimPos.Store.Domain.Models.Response;
using DimPos.Store.Infrastructure.Persistence;
using DimPos.Store.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.Store.Application.Features.Stores.Query.GetStoresByBrand;

public class GetStoresByBrandQueryHandler : IRequestHandler<GetStoresByBrandQuery, ApiResponse>
{
    private readonly IUnitOfWork<StoreContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    
    public GetStoresByBrandQueryHandler(IUnitOfWork<StoreContext> unitOfWork, ILogger logger, IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(GetStoresByBrandQuery request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if (brandId == Guid.Empty)
            throw new BadHttpRequestException("Không tìm thấy Id của thương hiệu");

        var stores = await _unitOfWork.GetRepository<Domain.Entities.Store>().GetPagingListAsync(
            selector: x => new GetStoresByBrandResponse()
            {
                Id = x.Id,
                Name = x.Name,
                Code = x.Code,
                Email = x.Email,
                Phone = x.Phone,
                Address = x.Address,
                Status = x.Status,
                ShortName = x.ShortName,
                Description = x.Description,
                Latitude = x.Latitude,
                Longitude = x.Longitude,
                WifiName = x.WifiName,
                WifiPassword = x.WifiPassword,
                Index = x.Index,
                LocalPasscode = x.LocalPasscode,
                ManagerName = x.ManagerName,
                StartingStoreCashLending = x.StartingStoreCashLending,
                Type = x.Type,
                CreatedDate = x.CreatedDate,
                LastModifiedDate = x.LastModifiedDate
            },
            predicate: x => x.BrandId == brandId &&
                            (string.IsNullOrEmpty(request.Name) || x.Name.Contains(request.Name)) &&
                            (string.IsNullOrEmpty(request.Code) || x.Code.Contains(request.Code)),
            page: request.Page,
            size: request.Size,
            sortBy: request.SortBy,
            isAsc: request.IsAsc
        );
        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Data = stores,
            Message = "Lấy danh sách cửa hàng theo thương hiệu thành công",
        };
    }
}