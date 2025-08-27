using DimPos.Brand.Application.Common.Protos;
using DimPos.Brand.Domain.Entities;
using DimPos.Brand.Infrastructure.Persistence;
using DimPos.Brand.Infrastructure.Repositories.Interface;
using DimPos.Brand.Infrastructure.Utils;
using Grpc.Core;

namespace DimPos.Brand.Application.GrpcService;
using DimPos.Brand.Application.Common;


public class BrandGrpcService : Common.Protos.BrandGrpcService.BrandGrpcServiceBase
{
    private readonly IUnitOfWork<BrandContext> _unitOfWork;
    private readonly ILogger _logger;

    public BrandGrpcService(IUnitOfWork<BrandContext> unitOfWork, ILogger logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public override async Task<GetBrandIdByAccountIdResponse> GetBrandIdByAccountId(GetBrandIdByAccountIdRequest request, ServerCallContext context)
    {
        _logger.Information($"BEGIN: {nameof(GetBrandIdByAccountId)} - {TimeUtil.GetCurrentSEATime()}");
        var accountId = Guid.Parse(request.AccountId);

        var brandAccount = await _unitOfWork.GetRepository<BrandAccounts>().SingleOrDefaultAsync(
            predicate: x => x.AccountId == accountId
        );
        if (brandAccount == null)
        {
            return new GetBrandIdByAccountIdResponse()
            {
                BrandId = String.Empty
            };
        }

        return new GetBrandIdByAccountIdResponse()
        {
            BrandId = brandAccount.BrandId.ToString()
        };
    }

    public override async Task<GetBrandImageByBrandIdResponse> GetBrandImageByBrandId(GetBrandImageByBrandIdRequest request, ServerCallContext context)
    {
        var brandId = Guid.Parse(request.BrandId);
        _logger.Information("BEGIN: {BrandImageByBrandIdName} - {CurrentSeaTime}", nameof(GetBrandImageByBrandId), TimeUtil.GetCurrentSEATime());
        
        var brand = await _unitOfWork.GetRepository<Brands>().SingleOrDefaultAsync(
            predicate: x => x.Id == brandId
        );
        if (brand == null)
        {
            _logger.Warning("Brand with ID {BrandId} not found.", brandId);
            throw new RpcException(new Status(StatusCode.NotFound, "Brand not found"));
        }
        _logger.Information("END: {BrandImageByBrandIdName} - {CurrentSeaTime}", nameof(GetBrandImageByBrandId), TimeUtil.GetCurrentSEATime());
        return new GetBrandImageByBrandIdResponse()
        {
            PictureUrl = brand.PictureUrl ?? String.Empty
        };
    }

    public override async Task<GetBrandDetailByIdResponse> GetBrandDetailById(GetBrandDetailByIdRequest request, ServerCallContext context)
    {
        var brandId = Guid.Parse(request.BrandId);
        _logger.Information("BEGIN: {GetBrandDetailByIdName} - {CurrentSeaTime}", nameof(GetBrandDetailById), TimeUtil.GetCurrentSEATime());
        
        var brand = await _unitOfWork.GetRepository<Brands>().SingleOrDefaultAsync(
            predicate: x => x.Id == brandId
        );
        if (brand == null)
        {
            _logger.Warning("Brand with ID {BrandId} not found.", brandId);
            throw new RpcException(new Status(StatusCode.NotFound, "Brand not found"));
        }
        _logger.Information("END: {GetBrandDetailByIdName} - {CurrentSeaTime}", nameof(GetBrandDetailById), TimeUtil.GetCurrentSEATime());
        return new GetBrandDetailByIdResponse()
        {
            Id = brand.Id.ToString(),
            Code = brand.Code,
            Name = brand.Name,
            Address = brand.Address,
            Phone = brand.Phone,
            Email = brand.Email ?? String.Empty,
            PictureUrl = brand.PictureUrl ?? String.Empty
        };
    }
}