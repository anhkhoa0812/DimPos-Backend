using DimPos.Brand.Application.Common.Protos;
using DimPos.Identity.Application.Common.Utils;
using DimPos.Identity.Application.Services.Interface;
using DimPos.Identity.Domain.Enum;
using DimPos.Identity.Domain.Models.Authentication;
using DimPos.Identity.Domain.Models.Common;
using DimPos.Identity.Infrastructure.Persistence;
using DimPos.Identity.Infrastructure.Repositories.Interface;
using DimPos.Store.Application.Common.Protos;
using Mediator;

namespace DimPos.Identity.Application.Features.Authentication.Command.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, ApiResponse>
{
    private readonly ILogger _logger;
    private readonly IUnitOfWork<IdentityContext> _unitOfWork;
    private readonly IAuthenticationService _authenticationService;
    private readonly BrandGrpcService.BrandGrpcServiceClient _brandGrpcService;
    private readonly StoreGrpcService.StoreGrpcServiceClient _storeGrpcService;
    public LoginCommandHandler(ILogger logger, IUnitOfWork<IdentityContext> unitOfWork, 
        IAuthenticationService authenticationService,
        BrandGrpcService.BrandGrpcServiceClient brandGrpcService,
        StoreGrpcService.StoreGrpcServiceClient storeGrpcService)

    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
        _brandGrpcService = brandGrpcService ?? throw new ArgumentNullException(nameof(brandGrpcService));
        _storeGrpcService = storeGrpcService ?? throw new ArgumentNullException(nameof(storeGrpcService));
    }
    public async ValueTask<ApiResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        _logger.Information($"BEGIN: {nameof(LoginCommandHandler)} - {DateTime.UtcNow}");
        var account = await _unitOfWork.GetRepository<Domain.Entities.Accounts>().SingleOrDefaultAsync(
            predicate: x => x.Username == request.Username
        );
        if (account == null)
        {
            return new ApiResponse()
            {
                Status = 404,
                Message = "Không tìm thấy tài khoản",
                Data = null
            };
        }
        var isValidPassword = PasswordUtil.Verify(request.Password, account.PasswordHash!, account.PasswordSalt!);
        if(!isValidPassword)
        {
            return new ApiResponse()
            {
                Status = 401,
                Message = "Sai tài khoản hoặc mật khẩu",
                Data = null
            };
        }

        string token = String.Empty;
        var role = await _unitOfWork.GetRepository<Domain.Entities.Role>().SingleOrDefaultAsync(
            predicate: x => x.Id == account.RoleId
        );
        switch (role.Name)
        {
            case ERoleName.BrandAdmin:
                var brandIdString = _brandGrpcService.GetBrandIdByAccountId(new GetBrandIdByAccountIdRequest()
                {
                    AccountId = account.Id.ToString()
                }).BrandId;
                
                if (string.IsNullOrEmpty(brandIdString))
                {
                    return new ApiResponse()
                    {
                        Status = 404,
                        Message = "Không tìm thấy thương hiệu",
                        Data = null
                    };
                }
                token = _authenticationService.GenerateAccessToken(account, role.Name, brandId: brandIdString, storeId: null);
                break;
            case ERoleName.Staff:
            case ERoleName.StoreAdmin:
                var storeIdString = _storeGrpcService.GetStoreIdByAccountId(new GetStoreIdByAccountIdRequest()
                {
                    AccountId = account.Id.ToString()
                }).StoreId;
                
                if (string.IsNullOrEmpty(storeIdString))
                {
                    return new ApiResponse()
                    {
                        Status = 404,
                        Message = "Không tìm thấy cửa hàng",
                        Data = null
                    };
                }
                token = _authenticationService.GenerateAccessToken(account, role.Name, storeId: storeIdString, brandId: null);
                break;
            default:
                token = _authenticationService.GenerateAccessToken(account, role.Name, brandId: null, storeId: null);
                break;
        }
        // var token = _authenticationService.GenerateAccessToken(account);
        var refreshToken = _authenticationService.GenerateRefreshToken();

        var response = new ApiResponse()
        {
            Status = 200,
            Message = "Đăng nhập thành công",
            Data = new LoginResponse()
            {
                AccountId = account.Id,
                Username = account.Username,
                AccessToken = token,
                RefreshToken = refreshToken,
            }
        };
        _logger.Information($"END: {nameof(LoginCommandHandler)} - {DateTime.UtcNow}");
        return response;
    }
}