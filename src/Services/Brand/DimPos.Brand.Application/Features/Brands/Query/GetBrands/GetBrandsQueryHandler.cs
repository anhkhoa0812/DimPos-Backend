using DimPos.Brand.Application.Services.Interface;
using DimPos.Brand.Domain.Models.Brand;
using DimPos.Brand.Domain.Models.Common;
using DimPos.Brand.Infrastructure.Persistence;
using DimPos.Brand.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.Brand.Application.Features.Brands.Query.GetBrands;

public class GetBrandsQueryHandler : IRequestHandler<GetBrandsQuery, ApiResponse>
{
    private readonly IUnitOfWork<BrandContext> _unitOfWork;
    private readonly ILogger _logger;
    
    public GetBrandsQueryHandler(IUnitOfWork<BrandContext> unitOfWork, ILogger logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }
    
    public async ValueTask<ApiResponse> Handle(GetBrandsQuery request, CancellationToken cancellationToken)
    {
        var brands = await _unitOfWork.GetRepository<Domain.Entities.Brands>().GetPagingListAsync(
            selector: x => new GetBrandsResponse()
            {
                Id = x.Id,
                Name = x.Name,
                Code = x.Code,
                Phone = x.Phone,
                Email = x.Email,
                Address = x.Address,
                PictureUrl = x.PictureUrl,
                Status = x.Status,
                ArchivedAt = x.ArchivedAt,
                CreatedDate = x.CreatedDate,
                LastModifiedDate = x.LastModifiedDate
            },
            predicate: x => (string.IsNullOrEmpty(request.Code) || x.Code.Contains(request.Code)) &&
                            (string.IsNullOrEmpty(request.Name) || x.Name.Contains(request.Name)),
            page: request.Page,
            size: request.Size,
            sortBy: request.SortBy ?? "CreatedDate",
            isAsc: request.IsAsc
        );
        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Lấy danh sách thương hiệu thành công",
            Data = brands
        };
    }
}