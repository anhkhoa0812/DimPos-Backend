using FluentValidation;

namespace DimPos.Catalog.Application.Features.ProductComboItems.Command.CreateProductComboItem;

public class CreateProductComboItemCommandValidator : AbstractValidator<CreateProductComboItemCommand>
{
    public CreateProductComboItemCommandValidator()
    {
        RuleFor(x => x.ProductVariantId)
            .NotEmpty().WithMessage("Id của sản phẩm combo là bắt buộc.")
            .NotNull().WithMessage("Id của sản phẩm combo là bắt buộc.")
            .NotEqual(Guid.Empty).WithMessage("Id của sản phẩm combo là bắt buộc.");

        RuleFor(x => x.ProductVariantItemId)
            .NotEmpty().WithMessage("Id của sản phẩm combo item là bắt buộc.")
            .NotNull().WithMessage("Id của sản phẩm combo item là bắt buộc.")
            .NotEqual(Guid.Empty).WithMessage("Id của sản phẩm combo item là bắt buộc.");

        RuleFor(x => x.Quantity)
            .NotNull().WithMessage("Số lượng sản phẩm combo item là bắt buộc.")
            .GreaterThan(0)
            .WithMessage("Số lượng sản phẩm combo item phải lớn hơn 0.");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0)
            .When(x => x.DisplayOrder.HasValue)
            .WithMessage("Thứ tự hiển thị của sản phẩm combo item phải lớn hơn hoặc bằng 0.");
    }
}