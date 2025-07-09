using System.Text.Json;
using DimPos.Store.Application.Common.Protos;
using DimPos.Store.Domain.Entities;
using DimPos.Store.Domain.Models.MPos;
using DimPos.Store.Infrastructure.Persistence;
using DimPos.Store.Infrastructure.Repositories.Interface;
using DimPos.Store.Infrastructure.Utils;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;

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
    public override async Task<GetStoresByBrandPagingResponse> GetStoresByBrandPaging(GetStoresByBrandPagingRequest request, ServerCallContext context)
    {
        var storesPagingByBrand = await _unitOfWork.GetRepository<Domain.Entities.Store>().GetPagingListAsync(
            predicate: x => x.BrandId == Guid.Parse(request.BrandId),
            page: request.Page,
            size: request.Size,
            isAsc: request.IsAsc,
            sortBy: request.SortBy
        );
        var response = new GetStoresByBrandPagingResponse();
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
                    Status = (StoreStatus) store.Status
                };
                response.Stores.Add(storeResponse);
            }
        }
        return response;
    }

    public override async Task<CheckStoresInBrandResponse> CheckStoresInBrand(CheckStoresInBrandRequest request, ServerCallContext context)
    {
        _logger.Information($"BEGIN: {nameof(CheckStoresInBrand)} - {TimeUtil.GetCurrentSEATime()}");
        var storeInBrandIds = await _unitOfWork.GetRepository<Domain.Entities.Store>().GetListAsync(
            selector: x => x.Id,
            predicate: x => x.BrandId == Guid.Parse(request.BrandId)
        );
        var requestedIds = request.ListStoreId.StoreId
            .Select(Guid.Parse)
            .ToList();
        var variantSet = new HashSet<Guid>(storeInBrandIds);
        bool allExist = requestedIds.All(variantSet.Contains);
        _logger.Information($"END: {nameof(CheckStoresInBrand)} - {TimeUtil.GetCurrentSEATime()}");
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

    public override async Task<GetStoreIdByAccountIdResponse> GetStoreIdByAccountId(GetStoreIdByAccountIdRequest request, ServerCallContext context)
    {
        var storeId = await _unitOfWork.GetRepository<Domain.Entities.StoreAccounts>().SingleOrDefaultAsync(
            selector: x => x.StoreId,
            predicate: x => x.AccountId == Guid.Parse(request.AccountId)
        );
        if (storeId == null)
        {
            return new GetStoreIdByAccountIdResponse()
            {
                StoreId = String.Empty
            };
        }
        return new GetStoreIdByAccountIdResponse()
        {
            StoreId = storeId.ToString()
        };
    }

    public override async Task<GetStoresByBrandIdResponse> GetStoresByBrandId(GetStoresByBrandIdRequest request, ServerCallContext context)
    {
        var brandId = Guid.Parse(request.BrandId);
        var requestedIds = request.StoreIds
            .Select(Guid.Parse)
            .ToList();
        var stores = await _unitOfWork.GetRepository<Domain.Entities.Store>().GetListAsync(
            predicate: x => x.BrandId == brandId && requestedIds.Contains(x.Id)
        );
        var response = new GetStoresByBrandIdResponse();
        foreach (var store in stores)
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
                Status = (StoreStatus) store.Status
            };
            response.Stores.Add(storeResponse);
        }
        return response;
    }

    public override async Task<GetTaxRateForStoreMenuResponse> GetTaxRateByStoreId(GetTaxRateForStoreMenuRequest request, ServerCallContext context)
    {
        var taxRate = await _unitOfWork.GetRepository<TaxRates>().SingleOrDefaultAsync(
            predicate: x => x.StoreId == Guid.Parse(request.StoreId) &&
                            x.BrandId == Guid.Parse(request.BrandId) && x.IsActive
        );
        if (taxRate == null)
        {
            return new GetTaxRateForStoreMenuResponse()
            {
                Id = string.Empty,
                Name = string.Empty,
                Rate = 0
            };
        }
        return new GetTaxRateForStoreMenuResponse()
        {
            Id = taxRate.Id.ToString(),
            Name = taxRate.Name,
            Rate = (float)(taxRate.Rate)
        };
    }

    public override async Task<GetTaxRateAndPaymentMethodConfigResponse> GetTaxRateAndPaymentMethodConfig(GetTaxRateAndPaymentMethodConfigRequest request, ServerCallContext context)
    {
        var store = await _unitOfWork.GetRepository<Domain.Entities.Store>().SingleOrDefaultAsync(
            predicate: x => x.Id == Guid.Parse(request.StoreId) && x.BrandId == Guid.Parse(request.BrandId),
            include: x => x.Include(x => x.TaxRates)
                .Include(x => x.StorePaymentMethodConfigs)
        );
        if (store == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Không tìm thấy cửa hàng"));
        }
        if(store.StorePaymentMethodConfigs == null || !store.StorePaymentMethodConfigs.Any())
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Không tìm thấy cấu hình phương thức thanh toán cho cửa hàng"));
        }
        
        var storePaymentMethodConfig = store.StorePaymentMethodConfigs.FirstOrDefault(
            x => x.IsActiveByStore 
            && x.StoreId == Guid.Parse(request.StoreId)
            && x.Id == Guid.Parse(request.StorePaymentMethodConfigId)
        );
        if (storePaymentMethodConfig == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Không tìm thấy cấu hình phương thức thanh toán cho cửa hàng"));
        }
        var taxRate = store.TaxRates.FirstOrDefault(x => x.IsActive);
        if (taxRate == null)
        {
            return new GetTaxRateAndPaymentMethodConfigResponse()
            {
                TaxRateId = String.Empty,
                TaxRateName = String.Empty,
                Rate = 0,
                SystemPaymentMethodId = storePaymentMethodConfig.SystemPaymentMethodTypeId.ToString(),
                CredentialsConfigAtStore = storePaymentMethodConfig.CredentialsConfigAtStore ?? String.Empty
            };
        }

        return new GetTaxRateAndPaymentMethodConfigResponse()
        {
            TaxRateId = taxRate.Id.ToString(),
            TaxRateName = taxRate.Name,
            Rate = (float)(taxRate.Rate),
            SystemPaymentMethodId = storePaymentMethodConfig.SystemPaymentMethodTypeId.ToString(),
            CredentialsConfigAtStore = storePaymentMethodConfig.CredentialsConfigAtStore ?? String.Empty
        };

    }

    public override async Task<GetCredentialsConfigByMerchantIdResponse> GetCredentialsConfigByMerchantId(GetCredentialsConfigByMerchantIdRequest request, ServerCallContext context)
    {
        var storePaymentMethodConfig = await _unitOfWork.GetRepository<StorePaymentMethodConfigs>().SingleOrDefaultAsync(
            predicate: x => x.CredentialsConfigAtStore != null
            && EF.Property<long>(x.CredentialsConfigAtStore, "MerchantId") == request.MerchantId
        );
        if (storePaymentMethodConfig == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Không tìm thấy cấu hình thanh toán cho MerchantId"));
        }

        return new GetCredentialsConfigByMerchantIdResponse()
        {
            StoreId = storePaymentMethodConfig.StoreId.ToString(),
            CredentialsConfig = storePaymentMethodConfig.CredentialsConfigAtStore ?? String.Empty
        };

    }

    public override async Task<GetBrandIdByStoreIdResponse> GetBrandIdByStoreId(GetBrandIdByStoreIdRequest request, ServerCallContext context)
    {
        var store = await _unitOfWork.GetRepository<Domain.Entities.Store>().SingleOrDefaultAsync(
            predicate: x => x.Id == Guid.Parse(request.StoreId)
        );
        return new GetBrandIdByStoreIdResponse()
        {
            BrandId = store.BrandId.ToString()
        };
    }
}