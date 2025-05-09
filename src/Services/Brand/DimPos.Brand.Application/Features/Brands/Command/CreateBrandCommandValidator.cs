using FluentValidation;

namespace DimPos.Brand.Application.Features.Brands.Command;

public class CreateBrandCommandValidator : AbstractValidator<CreateBrandCommand>
{
    public CreateBrandCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Mã của thương hiệu không được bỏ trống")
            .MaximumLength(50).WithMessage("Mã của thương hiệu không được vượt quá 50 ký tự");
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên của thương hiệu không được bỏ trống")
            .MaximumLength(200).WithMessage("Tên của thương hiệu không được vượt quá 200 ký tự");
        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Email không hợp lệ")
            .MaximumLength(100).WithMessage("Email không được vượt quá 100 ký tự");
        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Địa chỉ của thương hiệu không được bỏ trống")
            .MaximumLength(1000).WithMessage("Địa chỉ của thương hiệu không được vượt quá 1000 ký tự");
        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Số điện thoại của thương hiệu không được bỏ trống")
            .MaximumLength(20).WithMessage("Số điện thoại của thương hiệu không được vượt quá 20 ký tự");
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Tên đăng nhập không được bỏ trống")
            .MaximumLength(50).WithMessage("Tên đăng nhập không được vượt quá 50 ký tự");
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Mật khẩu không được bỏ trống")
            .MaximumLength(50).WithMessage("Mật khẩu không được vượt quá 50 ký tự");
    }
}