using FluentValidation;

namespace DimPos.Identity.Application.Features.Authentication.Command.Login;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Tên đăng nhập không được bỏ trống")
            .MaximumLength(50).WithMessage("Tên đăng nhập không được vượt quá 50 ký tự");
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Mật khẩu không được bỏ trống")
            .MaximumLength(50).WithMessage("Mật khẩu không được vượt quá 50 ký tự");
    }
}