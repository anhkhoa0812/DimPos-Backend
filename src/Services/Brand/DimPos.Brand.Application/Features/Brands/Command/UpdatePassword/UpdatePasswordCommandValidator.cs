using FluentValidation;

namespace DimPos.Brand.Application.Features.Brands.Command.UpdatePassword;

public class UpdatePasswordCommandValidator : AbstractValidator<UpdatePasswordCommand>
{
    public UpdatePasswordCommandValidator()
    {
        RuleFor(x => x.BrandId)
            .NotEmpty().WithMessage("Mã thương hiệu không được bỏ trống")
            .NotNull().WithMessage("Mã thương hiệu không được bỏ trống")
            .NotEqual(Guid.Empty).WithMessage("Mã thương hiệu không được là Guid.Empty");
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Mật khẩu không được bỏ trống")
            .MaximumLength(50).WithMessage("Mật khẩu không được vượt quá 50 ký tự");
    }
}