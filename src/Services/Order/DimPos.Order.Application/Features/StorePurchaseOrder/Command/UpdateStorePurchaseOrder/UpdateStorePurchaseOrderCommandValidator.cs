using FluentValidation;

namespace DimPos.Order.Application.Features.StorePurchaseOrder.Command.UpdateStorePurchaseOrder;

public class UpdateStorePurchaseOrderCommandValidator : AbstractValidator<UpdateStorePurchaseOrderCommand>
{
    public UpdateStorePurchaseOrderCommandValidator()
    {
        RuleFor(x => x.StorePurchaseOrderId)
            .NotEmpty().WithMessage("Mã đơn hàng không được bỏ trống")
            .NotNull().WithMessage("Mã đơn hàng không được bỏ trống")
            .NotEqual(Guid.Empty).WithMessage("Mã đơn hàng không được là Guid.Empty");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Trạng thái đơn hàng không hợp lệ");
        RuleFor(x => x.CancellationRequestReasonByStore)
            .MaximumLength(1000).WithMessage("Lý do hủy đơn hàng không được vượt quá 1000 ký tự");
        RuleFor(x => x.CancellationReasonByBrand)
            .MaximumLength(1000).WithMessage("Lý do hủy đơn hàng không được vượt quá 1000 ký tự");
    }
}