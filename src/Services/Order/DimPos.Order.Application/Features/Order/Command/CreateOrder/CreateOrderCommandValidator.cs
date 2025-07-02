using FluentValidation;

namespace DimPos.Order.Application.Features.Order.Command.CreateOrder;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.BrandId)
            .NotEqual(Guid.Empty)
            .WithMessage("Id của thương hiệu không được để trống.")
            .NotNull().WithMessage("Id của thương hiệu không được để trống.")
            .NotEmpty().WithMessage("Id của thương hiệu không được để trống.");
        RuleFor(x => x.PickupTime)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("Thời gian lấy hàng phải lớn hơn thời gian hiện tại.");
        
        RuleFor(x => x.Note)
            .MaximumLength(500)
            .WithMessage("Ghi chú không được vượt quá 500 ký tự.");
        RuleForEach(x => x.OrderItems)
            .NotEmpty().WithMessage("Danh sách mặt hàng không được để trống.")
            .NotNull().WithMessage("Danh sách mặt hàng không được để trống.")
            .SetValidator(new CreateOrderItemRequestValidator());
    }
}

public class CreateOrderItemRequestValidator : AbstractValidator<CreateOrderItemRequest>
{
    public CreateOrderItemRequestValidator()
    {
        RuleFor(x => x.ProductVariantId)
            .NotEqual(Guid.Empty)
            .WithMessage("Id của biến thể sản phẩm không được để trống.")
            .NotNull().WithMessage("Id của biến thể sản phẩm không được để trống.")
            .NotEmpty().WithMessage("Id của biến thể sản phẩm không được để trống.");
        RuleFor(x => x.Quantity)
            .NotEmpty().WithMessage("Số lượng không được để trống.")
            .NotNull().WithMessage("Số lượng không được để trống.")
            .GreaterThan(0).WithMessage("Số lượng phải lớn hơn 0.");
        RuleFor(x => x.Note)
            .MaximumLength(500)
            .WithMessage("Ghi chú không được vượt quá 500 ký tự.");
    }
}