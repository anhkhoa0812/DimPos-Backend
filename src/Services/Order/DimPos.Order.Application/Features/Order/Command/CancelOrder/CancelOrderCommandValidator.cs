using FluentValidation;

namespace DimPos.Order.Application.Features.Order.Command.CancelOrder;

public class CancelOrderCommandValidator : AbstractValidator<CancelOrderCommand>
{
    public CancelOrderCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Id của đơn hàng không được để trống")
            .NotNull().WithMessage("Id của đơn hàng không được để trống")
            .NotEqual(Guid.Empty).WithMessage("Id của đơn hàng không được để trống");

        RuleFor(x => x.CancellationReason)
            .NotEmpty().WithMessage("Lý do hủy đơn hàng không được để trống")
            .NotNull().WithMessage("Lý do hủy đơn hàng không được để trống");
    }
}