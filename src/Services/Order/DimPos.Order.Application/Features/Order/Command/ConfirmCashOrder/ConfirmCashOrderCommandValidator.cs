using FluentValidation;

namespace DimPos.Order.Application.Features.Order.Command.ConfirmCashOrder;

public class ConfirmCashOrderCommandValidator : AbstractValidator<ConfirmCashOrderCommand>
{
    public ConfirmCashOrderCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Id của đơn hàng không được để trống.")
            .NotNull().WithMessage("Id của đơn hàng không được để trống.")
            .NotEqual(Guid.Empty).WithMessage("Id của đơn hàng không được để trống.");

        RuleFor(x => x.AmountPaid)
            .NotNull().WithMessage("Số tiền thanh toán không được để trống.")
            .GreaterThan(0).WithMessage("Số tiền thanh toán phải lớn hơn 0.");
    }
}