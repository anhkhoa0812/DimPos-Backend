using DimPos.MenuCombo.Application.Services.Interface;
using DimPos.MenuCombo.Domain.Enums;
using DimPos.MenuCombo.Domain.Models.Common;
using DimPos.MenuCombo.Domain.Models.Stores;
using DimPos.MenuCombo.Infrastructure.Paginate;
using DimPos.MenuCombo.Infrastructure.Persistence;
using DimPos.MenuCombo.Infrastructure.Repositories.Interface;
using DimPos.Store.Application.Common.Protos;
using Mediator;

namespace DimPos.MenuCombo.Application.Features.BrandMenu.Query.GetStoresByMenu;

public class GetStoresByMenuQueryHandler : IRequestHandler<GetStoresByMenuQuery, ApiResponse>
{
    private readonly IUnitOfWork<MenuComboContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    private readonly StoreGrpcService.StoreGrpcServiceClient _storeGrpcService;
    public GetStoresByMenuQueryHandler(IUnitOfWork<MenuComboContext> unitOfWork,
        ILogger logger, IClaimService claimService,
        StoreGrpcService.StoreGrpcServiceClient storeGrpcService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
        _storeGrpcService = storeGrpcService ?? throw new ArgumentNullException(nameof(storeGrpcService));
    }
    
    public async ValueTask<ApiResponse> Handle(GetStoresByMenuQuery request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if(brandId == Guid.Empty)
            throw new BadHttpRequestException("Không tìm thấy brandId");
        var brandMenu = await _unitOfWork.GetRepository<Domain.Entities.BrandMenu>().SingleOrDefaultAsync(
            predicate: x => x.BrandId == brandId && x.Id == request.BrandMenuId
        );
        if (brandMenu == null)
            throw new BadHttpRequestException("Không tìm thấy BrandMenu");
        var storeGrpcResponse = await _storeGrpcService.GetStoresByBrandPagingAsync(new GetStoresByBrandPagingRequest()
        {
            BrandId = brandId.ToString(),
            Page = request.Page,
            Size = request.Size,
            SortBy = request.SortBy ?? String.Empty,
            IsAsc = request.IsAsc,
        });
        var response = new List<StoreByBrandResponse>();
        foreach (var store in storeGrpcResponse.Stores)
        {
            var storeResponse = new StoreByBrandResponse()
            {
                Id = Guid.Parse(store.Id),
                Name = store.Name,
                Description = store.Description,
                Address = store.Address,
                Email = store.Email,
                Phone = store.Phone,
                Latitude = store.Latitude,
                Longitude = store.Longitude,
                Status = (EStoreStatus) store.Status,
            };
            response.Add(storeResponse);
        }
        return new ApiResponse()
        {
            Status = 200,
            Message = "Lấy dữ liệu thành công",
            Data = new Paginate<StoreByBrandResponse>()
            {
                Page = request.Page,
                Size = request.Size,
                Items = response,
                Total = storeGrpcResponse.Total,
                TotalPages = storeGrpcResponse.TotalPages
            }
        };
    }
}