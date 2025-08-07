using DimPos.Identity.Application.Common.Protos;
using DimPos.Identity.Domain.Entities;
using DimPos.Identity.Domain.Enum;
using DimPos.Identity.Infrastructure.Persistence;
using DimPos.Identity.Infrastructure.Repositories.Interface;
using Grpc.Core;

namespace DimPos.Identity.Application.GrpcService;

public class IdentityGrpcService : Common.Protos.IdentityGrpcService.IdentityGrpcServiceBase
{
    private readonly IUnitOfWork<IdentityContext> _unitOfWork;
    private readonly ILogger _logger;
    
    public IdentityGrpcService(IUnitOfWork<IdentityContext> unitOfWork, ILogger logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override async Task<GetStaffDetailResponse> GetStaffDetail(GetStaffDetailRequest request, ServerCallContext context)
    {
        var accountIds = request.AccountId.Select(Guid.Parse).ToList();
        var staffs = await _unitOfWork.GetRepository<Accounts>().GetListAsync(
            predicate: x => accountIds.Contains(x.Id) && x.Role.Name == ERoleName.Staff
        );
        if(accountIds.Count != staffs.Count)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Không tìm thấy thông tin nhân viên với {accountIds.Count} và {staffs.Count} tài khoản"));
        }
        
        var response = new GetStaffDetailResponse()
        {
            Staffs =
            {
                staffs.Select(x => new StaffDetail()
                {
                    Id = x.Id.ToString(),
                    Code = x.Code,
                    Username = x.Username,
                    Email = x.Email ?? String.Empty,
                    Status = (AccountStatus) x.Status
                })
            }
        };
        return response;
    }
}