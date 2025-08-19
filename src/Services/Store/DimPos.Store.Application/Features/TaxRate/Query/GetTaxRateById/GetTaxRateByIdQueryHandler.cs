using DimPos.Store.Application.Services.Interface;
using DimPos.Store.Domain.Entities;
using DimPos.Store.Domain.Models.Common;
using DimPos.Store.Domain.Models.Response;
using DimPos.Store.Infrastructure.Persistence;
using DimPos.Store.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.Store.Application.Features.TaxRate.Query.GetTaxRateById;

public class GetTaxRateByIdQueryHandler : IRequestHandler<GetTaxRateByIdQuery, ApiResponse>
{
    private readonly IUnitOfWork<StoreContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    
    public GetTaxRateByIdQueryHandler(IUnitOfWork<StoreContext> unitOfWork, ILogger logger, IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(GetTaxRateByIdQuery request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId;
        var storeId = _claimService.GetStoreId;

        var taxRate = await _unitOfWork.GetRepository<TaxRates>().SingleOrDefaultAsync(
            predicate: x => x.Id == request.TaxRateId
            && (storeId == Guid.Empty || x.StoreId == storeId)
            && (brandId == Guid.Empty || x.BrandId == brandId)
        );
        if (taxRate == null)
        {
            _logger.Error("Không tìm thấy thuế với Id {TaxRateId} cho cửa hàng {StoreId}", request.TaxRateId, storeId);
            throw new BadHttpRequestException("Không tìm thấy thuế với Id này.");
        }
        
        _logger.Information("BEGIN: GetTaxRateByIdQueryHandler.Handle - TaxRateId: {TaxRateId}", request.TaxRateId);
        var response = new GetTaxRateByIdResponse()
        {
            Id = taxRate.Id,
            Name = taxRate.Name,
            Rate = taxRate.Rate,
            IsActive = taxRate.IsActive,
            CreatedDate = taxRate.CreatedDate,
            LastModifiedDate = taxRate.LastModifiedDate
        };
        _logger.Information("END: GetTaxRateByIdQueryHandler.Handle - TaxRateId: {TaxRateId}", request.TaxRateId);
        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Lấy thông tin thuế thành công",
            Data = response,
        };
    }
}