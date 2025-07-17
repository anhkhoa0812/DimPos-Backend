using DimPos.Identity.Application.Common.Protos;
using DimPos.Store.Application.Services.Interface;
using DimPos.Store.Domain.Entities;
using DimPos.Store.Domain.Enums;
using DimPos.Store.Domain.Models.Common;
using DimPos.Store.Domain.Models.Response;
using DimPos.Store.Infrastructure.Persistence;
using DimPos.Store.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.Store.Application.Features.Stores.Query.GetStaffById;

public class GetStaffByIdQueryHandler : IRequestHandler<GetStaffByIdQuery, ApiResponse>
{
    private readonly IUnitOfWork<StoreContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    private readonly IdentityGrpcService.IdentityGrpcServiceClient _identityGrpcService;
    
    public GetStaffByIdQueryHandler(IUnitOfWork<StoreContext> unitOfWork, ILogger logger, IClaimService claimService,
        IdentityGrpcService.IdentityGrpcServiceClient identityGrpcService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
        _identityGrpcService = identityGrpcService ?? throw new ArgumentNullException(nameof(identityGrpcService));
    }
    
    public async ValueTask<ApiResponse> Handle(GetStaffByIdQuery request, CancellationToken cancellationToken)
    {
        var storeId = _claimService.GetStoreId ?? Guid.Empty;
        if (storeId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy cửa hàng");
        }

        var storeAccount = await _unitOfWork.GetRepository<StoreAccounts>().SingleOrDefaultAsync(
            predicate: x => x.StoreId == storeId
            && x.AccountId == request.StaffId
            && x.Role ==EStoreRole.Staff
        );

        if (storeAccount == null)
        {
            throw new BadHttpRequestException("Nhân viên không tồn tại trong cửa hàng này");
        }
        
        var staffDetailResponse = await _identityGrpcService.GetStaffDetailAsync(new GetStaffDetailRequest()
        {
            AccountId = { request.StaffId.ToString() }
        });
        var staff = staffDetailResponse.Staffs.First();
        var response = new GetStaffByIdResponse()
        {
            Id = Guid.Parse(staff.Id),
            Code = staff.Code,
            Email = staff.Email,
            Status = (EAccountStatus)staff.Status,
            Username = staff.Username,
            AssignAt = storeAccount.AssignAt
        };
        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Lấy thông tin nhân viên thành công",
            Data = response
        };
    }
}