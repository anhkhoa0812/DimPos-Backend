using FluentValidation;

namespace DimPos.Store.Application.Features.Stores.Command.UpdateStaff;

public class UpdateStaffCommandValidator : AbstractValidator<UpdateStaffCommand>
{
    public UpdateStaffCommandValidator()
    {
        RuleFor(x => x.StaffId)
            .NotEmpty()
            .WithMessage("Mã nhân viên không được để trống.")
            .NotNull().WithMessage("Mã nhân viên không được để trống.")
            .NotEqual(Guid.Empty).WithMessage("Mã nhân viên không được là Guid.Empty.");
        RuleFor(x => x.Code)
            .MaximumLength(50)
            .WithMessage("Mã nhân viên không được vượt quá 50 ký tự.");
        RuleFor(x => x.Username)
            .MaximumLength(50)
            .WithMessage("Tên đăng nhập không được vượt quá 50 ký tự.");
        RuleFor(x => x.Password)
            .MaximumLength(50).WithMessage("Mật khẩu không được vượt quá 50 ký tự");
    }
}