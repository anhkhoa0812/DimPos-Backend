using FluentValidation;

namespace DimPos.Store.Application.Features.StorePaymentMethodConfig.Command.UpdateStorePaymentConfig;

public class UpdateStorePaymentConfigCommandValidator : AbstractValidator<UpdateStorePaymentConfigCommand>
{
    public UpdateStorePaymentConfigCommandValidator()
    {
        RuleFor(x => x.StorePaymentMethodConfigId)
            .NotEqual(Guid.Empty).WithMessage("StorePaymentMethodConfigId không được để trống")
            .NotEmpty().WithMessage("StorePaymentMethodConfigId không được bỏ trống")
            .NotNull().WithMessage("StorePaymentMethodConfigId không được bỏ trống");
        RuleFor(x => x.IsActiveByStore)
            .NotNull().WithMessage("IsActiveByStore không được bỏ trống");
    }
}