using FluentValidation;

namespace DimPos.Store.Application.Features.Stores.Command.CreateStore;

public class CreateStoreCommandValidator : AbstractValidator<CreateStoreCommand>
{
    public CreateStoreCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Mã cửa hàng không được để trống")
            .MaximumLength(50).WithMessage("Mã cửa hàng không được vượt quá 50 ký tự");
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên cửa hàng không được để trống")
            .MaximumLength(500).WithMessage("Tên cửa hàng không được vượt quá 500 ký tự");
        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Số điện thoại không được vượt quá 20 ký tự");
        RuleFor(x => x.Email)
            .MaximumLength(100).WithMessage("Email không được vượt quá 100 ký tự");
        RuleFor(x => x.ShortName)
            .MaximumLength(100).WithMessage("Tên ngắn của cửa hàng không được vượt quá 100 ký tự");
        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Mô tả cửa hàng không được vượt quá 1000 ký tự");
        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Địa chỉ cửa hàng không được để trống")
            .MaximumLength(1000).WithMessage("Địa chỉ cửa hàng không được vượt quá 1000 ký tự");
        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Loại cửa hàng không hợp lệ");
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Tên đăng nhập không được bỏ trống")
            .MaximumLength(50).WithMessage("Tên đăng nhập không được vượt quá 50 ký tự");
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Mật khẩu không được bỏ trống")
            .MaximumLength(50).WithMessage("Mật khẩu không được vượt quá 50 ký tự");
        
    }
}