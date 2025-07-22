using FluentValidation;

namespace DimPos.Catalog.Application.Features.ProductVariants.Command.CreateProductVariant;

public class CreateProductVariantCommandValidator : AbstractValidator<CreateProductVariantCommand>
{
    public CreateProductVariantCommandValidator()
    {
        RuleFor(p => p.Code)
            .NotEmpty().WithMessage("Mã của biến thể sản phẩm không được bỏ trống")
            .MinimumLength(1).WithMessage("Mã của biến thể sản phẩm phải có ít nhất 1 ký tự")
            .MaximumLength(50).WithMessage("Mã của biến thể sản phẩm không được vượt quá 50 ký tự");
        RuleFor(p => p.Name)
            .NotEmpty().WithMessage("Tên của biến thể sản phẩm không được bỏ trống")
            .MinimumLength(1).WithMessage("Tên của biến thể sản phẩm phải có ít nhất 1 ký tự")
            .MaximumLength(200).WithMessage("Tên của biến thể sản phẩm không được vượt quá 200 ký tự");
        RuleFor(p => p.Price)
            .NotEmpty().WithMessage("Giá của biến thể sản phẩm không được bỏ trống");
        RuleFor(p => p.Description)
            .MaximumLength(1000).WithMessage("Mô tả của biến thể sản phẩm không được vượt quá 1000 ký tự");
    }
}