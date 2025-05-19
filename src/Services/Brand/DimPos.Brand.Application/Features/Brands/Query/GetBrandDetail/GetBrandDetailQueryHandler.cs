using DimPos.Brand.Application.Common.Mapper;
using DimPos.Brand.Application.Services.Interface;
using DimPos.Brand.Domain.Models.Common;
using DimPos.Brand.Infrastructure.Persistence;
using DimPos.Brand.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.Brand.Application.Features.Brands.Query.GetBrandDetail;

public class GetBrandDetailQueryHandler : IRequestHandler<GetBrandDetailQuery, ApiResponse>
{
    private readonly IUnitOfWork<BrandContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    
    public GetBrandDetailQueryHandler(IUnitOfWork<BrandContext> unitOfWork, ILogger logger, IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(GetBrandDetailQuery request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if(brandId == Guid.Empty) 
            throw new BadHttpRequestException("Không tìm thấy Id của thương hiệu");
        var brand = await _unitOfWork.GetRepository<Domain.Entities.Brands>().SingleOrDefaultAsync(
            predicate: x => x.Id == brandId
        );
        var response = BrandMapper.ToGetBrandDetailResponse(brand);
        return new ApiResponse
        {
            Status = StatusCodes.Status200OK,
            Message = "Lấy dữ liệu thành công",
            Data = response
        };
    }
}