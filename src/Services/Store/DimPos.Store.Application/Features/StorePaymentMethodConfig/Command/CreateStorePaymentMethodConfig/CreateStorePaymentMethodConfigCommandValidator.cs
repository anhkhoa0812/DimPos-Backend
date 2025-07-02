using FluentValidation;

namespace DimPos.Store.Application.Features.StorePaymentMethodConfig.Command.CreateStorePaymentMethodConfig;

public class CreateStorePaymentMethodConfigCommandValidator : AbstractValidator<CreateStorePaymentMethodConfigCommand>
{
    public CreateStorePaymentMethodConfigCommandValidator()
    {
        RuleFor(x => x.SystemPaymentMethodId)
            .NotEmpty().WithMessage("Id phương thức thanh toán hệ thống không được để trống")
            .NotNull().WithMessage("Id phương thức thanh toán hệ thống không được để trống")
            .NotEqual(Guid.Empty).WithMessage("Id phương thức thanh toán hệ thống không được là Guid.Empty");
    }
}