using FluentValidation;

namespace DimPos.Order.Application.Features.StorePurchaseOrder.Command.CreateStorePurchaseOrder;

public class CreateStorePurchaseOrderCommandValidator : AbstractValidator<CreateStorePurchaseOrderCommand>
{
    public CreateStorePurchaseOrderCommandValidator()
    {
        RuleFor(x => x.Note)
            .MaximumLength(500).WithMessage("Ghi chú không được vượt quá 500 ký tự");
        RuleForEach(x => x.StorePurchaseOrderItems)
            .SetValidator(new CreateStorePurchaseOrderItemRequestValidator());
    }
}
public class CreateStorePurchaseOrderItemRequestValidator : AbstractValidator<CreateStorePurchaseOrderItemRequest>
{
    public CreateStorePurchaseOrderItemRequestValidator()
    {
        RuleFor(x => x.ProductVariantId)
            .NotEmpty().WithMessage("Mã biến thể sản phẩm không được bỏ trống")
            .NotNull().WithMessage("Mã biến thể sản phẩm không được bỏ trống")
            .NotEqual(Guid.Empty).WithMessage("Mã biến thể sản phẩm không được là Guid.Empty");
        
        RuleFor(x => x.RequestedQuantity)
            .GreaterThan(0).WithMessage("Số lượng phải lớn hơn 0")
            .NotNull().WithMessage("Số lượng không được bỏ trống")
            .NotEmpty().WithMessage("Số lượng không được bỏ trống");
    }
}