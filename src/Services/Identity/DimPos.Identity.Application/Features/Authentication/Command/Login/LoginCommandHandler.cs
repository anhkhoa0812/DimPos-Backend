using DimPos.Identity.Application.Common.Utils;
using DimPos.Identity.Application.Services.Interface;
using DimPos.Identity.Domain.Models.Authentication;
using DimPos.Identity.Domain.Models.Common;
using DimPos.Identity.Infrastructure.Persistence;
using DimPos.Identity.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.Identity.Application.Features.Authentication.Command.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, ApiResponse>
{
    private readonly ILogger _logger;
    private readonly IUnitOfWork<IdentityContext> _unitOfWork;
    private readonly IAuthenticationService _authenticationService;
    public LoginCommandHandler(ILogger logger, IUnitOfWork<IdentityContext> unitOfWork, IAuthenticationService authenticationService)

    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
    }
    public async ValueTask<ApiResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
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
        var token = _authenticationService.GenerateAccessToken(account);
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
        return response;
    }
}