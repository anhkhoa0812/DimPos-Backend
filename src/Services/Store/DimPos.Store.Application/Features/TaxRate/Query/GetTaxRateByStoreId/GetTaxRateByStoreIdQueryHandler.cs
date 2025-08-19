using DimPos.Store.Application.Services.Interface;
using DimPos.Store.Domain.Entities;
using DimPos.Store.Domain.Models.Common;
using DimPos.Store.Domain.Models.Response;
using DimPos.Store.Infrastructure.Persistence;
using DimPos.Store.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.Store.Application.Features.TaxRate.Query.GetTaxRateByStoreId;

public class GetTaxRateByStoreIdQueryHandler : IRequestHandler<GetTaxRateByStoreIdQuery, ApiResponse>
{
    private readonly IUnitOfWork<StoreContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    
    public GetTaxRateByStoreIdQueryHandler(IUnitOfWork<StoreContext> unitOfWork, ILogger logger, IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(GetTaxRateByStoreIdQuery request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        
        if (brandId == Guid.Empty)
            throw new BadHttpRequestException("Không tìm thấy thông tin thương hiệu trong yêu cầu.");
        
        _logger.Information("BEGIN: GetTaxRateByStoreIdQueryHandler.Handle - StoreId: {StoreId}", request.StoreId);
        var taxRates = await _unitOfWork.GetRepository<TaxRates>().GetPagingListAsync(
            selector: x => new GetTaxRateByStoreIdResponse()
            {
                Id = x.Id,
                Name = x.Name,
                Rate = x.Rate,
                IsActive = x.IsActive,
                CreatedDate = x.CreatedDate,
                LastModifiedDate = x.LastModifiedDate
            },
            predicate: x => x.StoreId == request.StoreId && x.BrandId == brandId,
            page: request.Page,
            size: request.Size,
            sortBy: request.SortBy ?? "CreatedDate",
            isAsc: request.IsAsc
        );

        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Lấy danh sách thuế thành công",
            Data = taxRates,
        };
    }
}