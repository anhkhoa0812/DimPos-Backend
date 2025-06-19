using FluentValidation;

namespace DimPos.Store.Application.Features.Stores.Command.CreateStaff;

public class CreateStaffCommandValidator : AbstractValidator<CreateStaffCommand>
{
    public CreateStaffCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Mã tài khoản không được để trống")
            .MaximumLength(50).WithMessage("Mã tài khoản không được vượt quá 50 ký tự");
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Tên đăng nhập không được bỏ trống")
            .MaximumLength(50).WithMessage("Tên đăng nhập không được vượt quá 50 ký tự");
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Mật khẩu không được bỏ trống")
            .MaximumLength(50).WithMessage("Mật khẩu không được vượt quá 50 ký tự");
        RuleFor(x => x.Email)
            .MaximumLength(100).WithMessage("Email không được vượt quá 100 ký tự");
    }
}