using DimPos.Identity.Application.Common.Protos;
using DimPos.Store.Application.Services.Interface;
using DimPos.Store.Domain.Entities;
using DimPos.Store.Domain.Enums;
using DimPos.Store.Domain.Models.Common;
using DimPos.Store.Domain.Models.Response;
using DimPos.Store.Infrastructure.Persistence;
using DimPos.Store.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.Store.Application.Features.Stores.Query.GetStaffs;

public class GetStaffsQueryHandler : IRequestHandler<GetStaffsQuery, ApiResponse>
{
    private readonly IUnitOfWork<StoreContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    private readonly IdentityGrpcService.IdentityGrpcServiceClient _identityGrpcService;
    
    public GetStaffsQueryHandler(IUnitOfWork<StoreContext> unitOfWork, ILogger logger, IClaimService claimService,
        IdentityGrpcService.IdentityGrpcServiceClient identityGrpcService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
        _identityGrpcService = identityGrpcService ?? throw new ArgumentNullException(nameof(identityGrpcService));
    }
    
    public async ValueTask<ApiResponse> Handle(GetStaffsQuery request, CancellationToken cancellationToken)
    {
        var storeId = _claimService.GetStoreId ?? Guid.Empty;
        if (storeId == Guid.Empty)
            throw new BadHttpRequestException("Không tìm thấy thông tin cửa hàng trong yêu cầu.");

        var staffAccounts = await _unitOfWork.GetRepository<StoreAccounts>().GetListAsync(
            predicate: x => x.StoreId == storeId && x.Role == EStoreRole.Staff
        );
        if (staffAccounts == null || !staffAccounts.Any())
        {
            return new ApiResponse
            {
                Status = StatusCodes.Status200OK,
                Message = "Lấy danh sách nhân viên thành công, nhưng không có nhân viên nào được tìm thấy.",
                Data = new List<GetStaffsResponse>()
            };
        }

        var staffDetailResponse = await _identityGrpcService.GetStaffDetailAsync(new GetStaffDetailRequest()
        {
            AccountId =
            {
                staffAccounts.Select(x => x.AccountId.ToString()).ToList()
            }
        });
        var response = staffDetailResponse.Staffs.Select(x => new GetStaffsResponse()
        {
            Id = Guid.Parse(x.Id),
            Code = x.Code,
            Email = x.Email,
            Status = (EAccountStatus)x.Status,
            Username = x.Username,
            AssignAt = staffAccounts.First(y => y.AccountId == Guid.Parse(x.Id)).AssignAt
        }).ToList();

        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Lấy thông tin nhân viên thành công",
            Data = response
        };
    }
}