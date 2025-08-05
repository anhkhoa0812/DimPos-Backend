using FluentValidation;

namespace DimPos.Order.Application.Features.Order.Command.UpdatePaymentMethod;

public class UpdatePaymentMethodCommandValidator : AbstractValidator<UpdatePaymentMethodCommand>
{
    public UpdatePaymentMethodCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEqual(Guid.Empty)
            .WithMessage("Id của đơn hàng không được để trống.")
            .NotNull().WithMessage("Id của đơn hàng không được để trống.")
            .NotEmpty().WithMessage("Id của đơn hàng không được để trống.");

        RuleFor(x => x.NewStorePaymentMethodConfigId)
            .NotEqual(Guid.Empty)
            .WithMessage("Id của cấu hình phương thức thanh toán cửa hàng mới không được để trống.")
            .NotNull().WithMessage("Id của cấu hình phương thức thanh toán cửa hàng mới không được để trống.")
            .NotEmpty().WithMessage("Id của cấu hình phương thức thanh toán cửa hàng mới không được để trống.");
        RuleFor(x => x.OldStorePaymentMethodConfigId)
            .NotEqual(Guid.Empty)
            .WithMessage("Id của cấu hình phương thức thanh toán cửa hàng cũ không được để trống.")
            .NotNull().WithMessage("Id của cấu hình phương thức thanh toán cửa hàng cũ không được để trống.")
            .NotEmpty().WithMessage("Id của cấu hình phương thức thanh toán cửa hàng cũ không được để trống.");
    }
}