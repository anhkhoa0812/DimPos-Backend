using DimPos.Store.Application.Common.Protos;
using DimPos.Store.Infrastructure.Persistence;
using DimPos.Store.Infrastructure.Repositories.Interface;
using Grpc.Core;

namespace DimPos.Store.Application.GrpcServices;

public class StoreGrpcService : Common.Protos.StoreGrpcService.StoreGrpcServiceBase
{
    private readonly IUnitOfWork<StoreContext> _unitOfWork;
    private readonly ILogger _logger;
    
    public StoreGrpcService(IUnitOfWork<StoreContext> unitOfWork, ILogger logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    public override async Task<GetStoresByBrandResponse> GetStoresByBrand(GetStoresByBrandRequest request, ServerCallContext context)
    {
        var storesPagingByBrand = await _unitOfWork.GetRepository<Domain.Entities.Store>().GetPagingListAsync(
            predicate: x => x.BrandId == Guid.Parse(request.BrandId),
            page: request.Page,
            size: request.Size,
            isAsc: request.IsAsc,
            sortBy: request.SortBy
        );
        var response = new GetStoresByBrandResponse();
        response.Total = storesPagingByBrand.Total;
        response.TotalPages = storesPagingByBrand.TotalPages;
        if (storesPagingByBrand != null)
        {
            foreach (var store in storesPagingByBrand.Items)
            {
                var storeResponse = new StoreResponse()
                {
                    Id = store.Id.ToString(),
                    Name = store.Name ?? String.Empty,
                    Description = store.Description ?? String.Empty,
                    Address = store.Address ?? String.Empty,
                    Email = store.Email ?? String.Empty,
                    Phone = store.Phone ?? String.Empty,
                    Latitude = store.Latitude ?? String.Empty,
                    Longitude = store.Longitude ?? String.Empty,
                };
                response.Stores.Add(storeResponse);
            }
        }
        return response;
    }

    public override async Task<CheckStoresInBrandResponse> CheckStoresInBrand(CheckStoresInBrandRequest request, ServerCallContext context)
    {
        _logger.Information($"BEGIN: {nameof(CheckStoresInBrand)} - {DateTime.UtcNow}");
        var storeInBrandIds = await _unitOfWork.GetRepository<Domain.Entities.Store>().GetListAsync(
            selector: x => x.Id,
            predicate: x => x.BrandId == Guid.Parse(request.BrandId)
        );
        var requestedIds = request.ListStoreId.StoreId
            .Select(Guid.Parse)
            .ToList();
        var variantSet = new HashSet<Guid>(storeInBrandIds);
        bool allExist = requestedIds.All(variantSet.Contains);
        _logger.Information($"END: {nameof(CheckStoresInBrand)} - {DateTime.UtcNow}");
        if (!allExist)
        {
            return new CheckStoresInBrandResponse()
            {
                IsValid = false,
            };
        }
        return new CheckStoresInBrandResponse()
        {
            IsValid = true
        };
    }
}