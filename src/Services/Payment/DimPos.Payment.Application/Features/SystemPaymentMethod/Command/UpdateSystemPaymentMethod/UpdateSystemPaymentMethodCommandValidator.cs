using FluentValidation;

namespace DimPos.Payment.Application.Features.SystemPaymentMethod.Command.UpdateSystemPaymentMethod;

public class UpdateSystemPaymentMethodCommandValidator : AbstractValidator<UpdateSystemPaymentMethodCommand>
{
    public UpdateSystemPaymentMethodCommandValidator()
    {
        RuleFor(x => x.SystemPaymentMethodId)
            .NotEmpty().WithMessage("Id của phương thức thanh toán hệ thống không được để trống.")
            .NotNull().WithMessage("Id của phương thức thanh toán hệ thống không được để trống.")
            .NotEqual(Guid.Empty).WithMessage("Id của phương thức thanh toán hệ thống không được để trống.");
        
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên phương thức thanh toán hệ thống không được để trống.")
            .MaximumLength(500).WithMessage("Tên phương thức thanh toán hệ thống không được vượt quá 500 ký tự.");
    }
}