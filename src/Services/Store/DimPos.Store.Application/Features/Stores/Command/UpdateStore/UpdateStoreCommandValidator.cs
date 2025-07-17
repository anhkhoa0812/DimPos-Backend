using FluentValidation;

namespace DimPos.Store.Application.Features.Stores.Command.UpdateStore;

public class UpdateStoreCommandValidator : AbstractValidator<UpdateStoreCommand>
{
    public UpdateStoreCommandValidator()
    {
        RuleFor(x => x.Code)
            .MaximumLength(50).WithMessage("Mã cửa hàng không được vượt quá 50 ký tự");
        RuleFor(x => x.Name)
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
            .MaximumLength(1000).WithMessage("Địa chỉ cửa hàng không được vượt quá 1000 ký tự");
        RuleFor(x => x.StartingStoreCashLending)
            .GreaterThanOrEqualTo(0).WithMessage("Số tiền cho vay ban đầu của cửa hàng phải lớn hơn hoặc bằng 0");
        RuleFor(x => x.Username)
            .MaximumLength(50).WithMessage("Tên đăng nhập không được vượt quá 50 ký tự");
        RuleFor(x => x.Password)
            .MaximumLength(50).WithMessage("Mật khẩu không được vượt quá 50 ký tự");
    }
}