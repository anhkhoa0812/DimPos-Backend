using DimPos.Brand.Application.Common.Protos;
using DimPos.Brand.Domain.Entities;
using DimPos.Brand.Infrastructure.Persistence;
using DimPos.Brand.Infrastructure.Repositories.Interface;
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
        _logger.Information($"BEGIN: {nameof(GetBrandIdByAccountId)} - {DateTime.UtcNow}");
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
}