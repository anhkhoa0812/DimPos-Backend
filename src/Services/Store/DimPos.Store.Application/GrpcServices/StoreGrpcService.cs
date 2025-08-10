using System.Text.Json;
using DimPos.Store.Application.Common.Protos;
using DimPos.Store.Domain.Entities;
using DimPos.Store.Domain.Enums;
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
            predicate: x => x.BrandId == Guid.Parse(request.BrandId) &&
                           (string.IsNullOrEmpty(request.Name) || x.Name.Contains(request.Name)) && 
                           (string.IsNullOrEmpty(request.Code) || x.Code.Contains(request.Code)),
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
                    Status = (StoreStatus) store.Status,
                    Code = store.Code ?? String.Empty
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
                Status = (StoreStatus) store.Status,
                Code = store.Code ?? String.Empty
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
                .Include(x => x.FinancialShiftConfigs)
                .ThenInclude(x => x.FinancialShifts)
        );
        
        if (store == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Không tìm thấy cửa hàng"));
        }
        
        var activeFinancialShiftConfig = store.FinancialShiftConfigs?
            .FirstOrDefault(x => x.IsActive);
        if (activeFinancialShiftConfig == null)
        {
            return new GetTaxRateAndPaymentMethodConfigResponse()
            {
                IsSuccess = false,
                ErrorMessage = "Không tìm thấy cấu hình ca tài chính cho cửa hàng",
                TaxRateId = String.Empty,
                TaxRateName = String.Empty,
                Rate = 0,
                FinancialShiftId = String.Empty,
                CredentialsConfigAtStore = String.Empty,
                SystemPaymentMethodId = String.Empty
            };
        }

        var financialShift =
            activeFinancialShiftConfig.FinancialShifts?.FirstOrDefault(x => x.Status == EFinancialShiftStatus.Open);
        if (financialShift == null)
        {
            return new GetTaxRateAndPaymentMethodConfigResponse()
            {
                IsSuccess = false,
                ErrorMessage = "Không tìm thấy ca tài chính đang mở cho cửa hàng",
                TaxRateId = String.Empty,
                TaxRateName = String.Empty,
                Rate = 0,
                FinancialShiftId = String.Empty,
                CredentialsConfigAtStore = String.Empty,
                SystemPaymentMethodId = String.Empty
            };
        }
        
        if(store.StorePaymentMethodConfigs == null || !store.StorePaymentMethodConfigs.Any())
        {
            return new GetTaxRateAndPaymentMethodConfigResponse()
            {
                IsSuccess = false,
                ErrorMessage = "Không tìm thấy cấu hình phương thức thanh toán cho cửa hàng",
                TaxRateId = String.Empty,
                TaxRateName = String.Empty,
                Rate = 0,
                FinancialShiftId = String.Empty,
                CredentialsConfigAtStore = String.Empty,
                SystemPaymentMethodId = String.Empty
            };
        }
        var nowTimeOnly = TimeOnly.FromDateTime(TimeUtil.GetCurrentSEATime());
        if(nowTimeOnly < activeFinancialShiftConfig.OpeningTime 
           || nowTimeOnly > activeFinancialShiftConfig.ClosingTime)
        {
            return new GetTaxRateAndPaymentMethodConfigResponse()
            {
                IsSuccess = false,
                ErrorMessage = "Ca tài chính hiện tại không hợp lệ, vui lòng kiểm tra lại thời gian mở ca và đóng ca",
                TaxRateId = String.Empty,
                TaxRateName = String.Empty,
                Rate = 0,
                FinancialShiftId = String.Empty,
                CredentialsConfigAtStore = String.Empty,
                SystemPaymentMethodId = String.Empty
            };
        }
        
        var storePaymentMethodConfig = store.StorePaymentMethodConfigs.FirstOrDefault(
            x => x.IsActiveByStore 
            && x.StoreId == Guid.Parse(request.StoreId)
            && x.Id == Guid.Parse(request.StorePaymentMethodConfigId)
        );
        if (storePaymentMethodConfig == null)
        {
            return new GetTaxRateAndPaymentMethodConfigResponse()
            {
                IsSuccess = false,
                ErrorMessage = "Không tìm thấy cấu hình phương thức thanh toán cho cửa hàng",
                TaxRateId = String.Empty,
                TaxRateName = String.Empty,
                Rate = 0,
                FinancialShiftId = financialShift.Id.ToString(),
                CredentialsConfigAtStore = String.Empty,
                SystemPaymentMethodId = String.Empty
            };
        }
        var taxRate = store.TaxRates.FirstOrDefault(x => x.IsActive);
        if (taxRate == null)
        {
            return new GetTaxRateAndPaymentMethodConfigResponse()
            {
                IsSuccess = true,
                ErrorMessage = String.Empty,
                TaxRateId = String.Empty,
                TaxRateName = String.Empty,
                Rate = 0,
                SystemPaymentMethodId = storePaymentMethodConfig.SystemPaymentMethodTypeId.ToString(),
                CredentialsConfigAtStore = storePaymentMethodConfig.CredentialsConfigAtStore ?? String.Empty,
                FinancialShiftId = financialShift.Id.ToString()
            };
        }

        return new GetTaxRateAndPaymentMethodConfigResponse()
        {
            IsSuccess = true,
            ErrorMessage = String.Empty,
            TaxRateId = taxRate.Id.ToString(),
            TaxRateName = taxRate.Name,
            Rate = (float)(taxRate.Rate),
            SystemPaymentMethodId = storePaymentMethodConfig.SystemPaymentMethodTypeId.ToString(),
            CredentialsConfigAtStore = storePaymentMethodConfig.CredentialsConfigAtStore ?? String.Empty,
            FinancialShiftId = financialShift.Id.ToString()
        };

    }

    public override async Task<GetCredentialsConfigByMerchantIdResponse> GetCredentialsConfigByMerchantId(GetCredentialsConfigByMerchantIdRequest request, ServerCallContext context)
    {
        var storePaymentMethodConfigs = await _unitOfWork.GetRepository<StorePaymentMethodConfigs>().GetListAsync(
            predicate: x => x.CredentialsConfigAtStore != null 
        );
        if (storePaymentMethodConfigs == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Không tìm thấy cấu hình phương thức thanh toán cho cửa hàng"));
        }

        var storePaymentMethodConfig = storePaymentMethodConfigs
            .FirstOrDefault(x =>
                JsonSerializer.Deserialize<MPosModelRequest>(x.CredentialsConfigAtStore)?.MerchantId ==
                request.MerchantId);
        if (storePaymentMethodConfig == null)
        {
            _logger.Warning("No StorePaymentMethodConfig found for MerchantId: {MerchantId}", request.MerchantId);
            throw new RpcException(new Status(StatusCode.NotFound, "Không tìm thấy cấu hình phương thức thanh toán cho cửa hàng"));
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

    public override async Task<GetListStoreByStoreIdsResponse> GetListStoreByStoreIds(GetListStoreByStoreIdsRequest request, ServerCallContext context)
    {
        var storeIds = request.StoreIds.Select(Guid.Parse).ToList();
        
        var stores = await _unitOfWork.GetRepository<Domain.Entities.Store>().GetListAsync(
            predicate: x => storeIds.Contains(x.Id)
        );
        if(storeIds.Count != stores.Count)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Không tìm thấy thông tin cửa hàng"));
        }
        
        var response = new GetListStoreByStoreIdsResponse();
        foreach (var store in stores)
        {
            var storeResponse = new StoreResponse()
            {
                Id = store.Id.ToString(),
                Code = store.Code,
                Name = store.Name,
                Description = store.Description ?? String.Empty,
                Address = store.Address,
                Email = store.Email ?? String.Empty,
                Phone = store.Phone ?? String.Empty,
                Latitude = store.Latitude ?? String.Empty,
                Longitude = store.Longitude ?? String.Empty,
                Status = (StoreStatus)store.Status
            };
            response.Stores.Add(storeResponse);
        }
        return response;
    }

    public override async Task<GetPaymentMethodConfigForUpdatePaymentMethodResponse> GetPaymentMethodConfigForUpdatePaymentMethod(GetPaymentMethodConfigForUpdatePaymentMethodRequest request,
        ServerCallContext context)
    {
        var store = await _unitOfWork.GetRepository<Domain.Entities.Store>().SingleOrDefaultAsync(
            predicate: x => x.Id == Guid.Parse(request.StoreId) && x.BrandId == Guid.Parse(request.BrandId),
            include: x => x.Include(x => x.StorePaymentMethodConfigs)
        );
        if (store == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Không tìm thấy cửa hàng"));
        }
        if (store.StorePaymentMethodConfigs == null || !store.StorePaymentMethodConfigs.Any())
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Không tìm thấy cấu hình phương thức thanh toán cho cửa hàng"));
        }

        var oldStorePaymentMethodConfig = store.StorePaymentMethodConfigs.FirstOrDefault(x => x.IsActiveByStore
            && x.StoreId == Guid.Parse(request.StoreId)
            && x.Id == Guid.Parse(request.OldStorePaymentMethodConfigId)
        );
        if (oldStorePaymentMethodConfig == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Không tìm thấy cấu hình phương thức thanh toán cũ cho cửa hàng"));
        }
        var newStorePaymentMethodConfig = store.StorePaymentMethodConfigs.FirstOrDefault(x => x.IsActiveByStore
            && x.StoreId == Guid.Parse(request.StoreId)
            && x.Id == Guid.Parse(request.NewStorePaymentMethodConfigId)
        );
        if (newStorePaymentMethodConfig == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Không tìm thấy cấu hình phương thức thanh toán mới cho cửa hàng"));
        }

        return new GetPaymentMethodConfigForUpdatePaymentMethodResponse()
        {
            OldSystemPaymentMethodId = oldStorePaymentMethodConfig.SystemPaymentMethodTypeId.ToString(),
            OldCredentialsConfigAtStore = oldStorePaymentMethodConfig.CredentialsConfigAtStore ?? String.Empty,
            NewSystemPaymentMethodId = newStorePaymentMethodConfig.SystemPaymentMethodTypeId.ToString(),
            NewCredentialsConfigAtStore = newStorePaymentMethodConfig.CredentialsConfigAtStore ?? String.Empty
        };
    }

    public override async Task<GetCredentialsConfigBySystemPaymentMethodIdResponse> GetCredentialsConfigBySystemPaymentMethodId(GetCredentialsConfigBySystemPaymentMethodIdRequest request,
        ServerCallContext context)
    {
        var storeId = Guid.Parse(request.StoreId);
        var systemPaymentMethodId = Guid.Parse(request.SystemPaymentMethodId);

        var storePaymentMethodConfig = await _unitOfWork.GetRepository<StorePaymentMethodConfigs>()
            .SingleOrDefaultAsync(
                predicate: x => x.SystemPaymentMethodTypeId == systemPaymentMethodId 
                                && x.StoreId == storeId
            );
        if (storePaymentMethodConfig == null)
        {
            return new GetCredentialsConfigBySystemPaymentMethodIdResponse()
            {
                IsSuccess = false,
                ErrorMessage = "Không tìm thấy cấu hình phương thức thanh toán cho cửa hàng",
                SystemPaymentMethodId = String.Empty,
                CredentialsConfigAtStore = String.Empty
            };
        }
        
        return new GetCredentialsConfigBySystemPaymentMethodIdResponse()
        {
            IsSuccess = true,
            ErrorMessage = String.Empty,
            SystemPaymentMethodId = storePaymentMethodConfig.SystemPaymentMethodTypeId.ToString(),
            CredentialsConfigAtStore = storePaymentMethodConfig.CredentialsConfigAtStore ?? String.Empty
        };
        
    }
}