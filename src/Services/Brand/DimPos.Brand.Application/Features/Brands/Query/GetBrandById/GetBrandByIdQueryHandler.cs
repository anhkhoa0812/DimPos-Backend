using DimPos.Brand.Domain.Models.Brand;
using DimPos.Brand.Domain.Models.Common;
using DimPos.Brand.Infrastructure.Persistence;
using DimPos.Brand.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.Brand.Application.Features.Brands.Query.GetBrandById;

public class GetBrandByIdQueryHandler : IRequestHandler<GetBrandByIdQuery, ApiResponse>
{
    private readonly IUnitOfWork<BrandContext> _unitOfWork;
    private readonly ILogger _logger;
    
    public GetBrandByIdQueryHandler(IUnitOfWork<BrandContext> unitOfWork, ILogger logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }
    
    public async ValueTask<ApiResponse> Handle(GetBrandByIdQuery request, CancellationToken cancellationToken)
    {
        var brand = await _unitOfWork.GetRepository<Domain.Entities.Brands>().SingleOrDefaultAsync(
            predicate: x => x.Id == request.BrandId
        );
        if (brand == null)
        {
            throw new BadHttpRequestException("Không tìm thấy thương hiệu với ID đã cho");
        }

        var response = new GetBrandByIdResponse()
        {
            Id = brand.Id,
            Code = brand.Code,
            Name = brand.Name,
            Email = brand.Email,
            Address = brand.Address,
            Phone = brand.Phone,
            PictureUrl = brand.PictureUrl,
            ArchivedAt = brand.ArchivedAt,
            Status = brand.Status,
            CreatedDate = brand.CreatedDate,
            LastModifiedDate = brand.LastModifiedDate
        };
        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Lấy thông tin thương hiệu thành công",
            Data = response
        };
    }
}