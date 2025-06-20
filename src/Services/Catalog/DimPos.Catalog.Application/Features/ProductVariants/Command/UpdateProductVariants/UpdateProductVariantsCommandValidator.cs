using FluentValidation;

namespace DimPos.Catalog.Application.Features.ProductVariants.Command.UpdateProductVariants;

public class UpdateProductVariantsCommandValidator : AbstractValidator<UpdateProductVariantsCommand>
{
    public UpdateProductVariantsCommandValidator()
    {
        RuleFor(p => p.ProductVariantId)
            .NotEmpty().WithMessage("ID của biến thể sản phẩm không được bỏ trống")
            .NotNull().WithMessage("ID của biến thể sản phẩm không được bỏ trống");
        RuleFor(p => p.UpdateProductVariants).SetValidator(new UpdateProductVariantsRequestValidator());
    }
}
public class UpdateProductVariantsRequestValidator : AbstractValidator<UpdateProductVariantsRequest>
{
    public UpdateProductVariantsRequestValidator()
    {
        RuleFor(p => p.AlternativeCode)
            .MaximumLength(100).WithMessage("Mã thay thế của biến thể sản phẩm không được vượt quá 100 ký tự");
        RuleFor(p => p.Name)
            .MinimumLength(1).WithMessage("Tên của biến thể sản phẩm phải có ít nhất 1 ký tự")
            .MaximumLength(200).WithMessage("Tên của biến thể sản phẩm không được vượt quá 200 ký tự");
        RuleFor(p => p.Sku)
            .MaximumLength(255).WithMessage("SKU của biến thể sản phẩm không được vượt quá 255 ký tự");
    }
}