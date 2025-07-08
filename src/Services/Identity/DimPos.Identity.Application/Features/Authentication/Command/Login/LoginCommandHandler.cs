using DimPos.Brand.Application.Common.Protos;
using DimPos.Identity.Application.Common.Exceptions;
using DimPos.Identity.Application.Common.Utils;
using DimPos.Identity.Application.Services.Interface;
using DimPos.Identity.Domain.Enum;
using DimPos.Identity.Domain.Models.Authentication;
using DimPos.Identity.Domain.Models.Common;
using DimPos.Identity.Infrastructure.Persistence;
using DimPos.Identity.Infrastructure.Repositories.Interface;
using DimPos.Identity.Infrastructure.Utils;
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
        _logger.Information($"BEGIN: {nameof(LoginCommandHandler)} - {TimeUtil.GetCurrentSEATime()}");
        var account = await _unitOfWork.GetRepository<Domain.Entities.Accounts>().SingleOrDefaultAsync(
            predicate: x => x.Username == request.Username
        );
        if (account == null)
        {
            throw new NotFoundException("Không tìm thấy tài khoản");
        }
        var isValidPassword = PasswordUtil.Verify(request.Password, account.PasswordHash!, account.PasswordSalt!);
        if(!isValidPassword)
        {
            throw new BadHttpRequestException("Sai tài khoản hoặc mật khẩu");
        }

        string token = String.Empty;
        var role = await _unitOfWork.GetRepository<Domain.Entities.Role>().SingleOrDefaultAsync(
            predicate: x => x.Id == account.RoleId
        );
        switch (role.Name)
        {
            case ERoleName.BrandAdmin:
                var brandGrpcResponse = await _brandGrpcService.GetBrandIdByAccountIdAsync(new GetBrandIdByAccountIdRequest()
                {
                    AccountId = account.Id.ToString()
                });
                
                if (string.IsNullOrEmpty(brandGrpcResponse.BrandId))
                    throw new NotFoundException("Không tìm thấy thương hiệu");
                token = _authenticationService.GenerateAccessToken(account, role.Name, brandId: brandGrpcResponse.BrandId, storeId: null);
                break;
            case ERoleName.Staff:
            case ERoleName.StoreAdmin:
                var storeGrpcResponse = await _storeGrpcService.GetStoreIdByAccountIdAsync(new GetStoreIdByAccountIdRequest()
                {
                    AccountId = account.Id.ToString()
                });
                
                if (string.IsNullOrEmpty(storeGrpcResponse.StoreId))
                    throw new NotFoundException("Không tìm thấy cửa hàng");
                
                token = _authenticationService.GenerateAccessToken(account, role.Name, storeId: storeGrpcResponse.StoreId, brandId: null);
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
        _logger.Information($"END: {nameof(LoginCommandHandler)} - {TimeUtil.GetCurrentSEATime()}");
        return response;
    }
}