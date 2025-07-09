using FluentValidation;

namespace DimPos.Catalog.Application.Features.InternalProducts.Command.UpdateInternalProduct;

public class UpdateInternalProductCommandValidator : AbstractValidator<UpdateInternalProductCommand>
{
    public UpdateInternalProductCommandValidator()
    {
        RuleFor(x => x.Code)
            .MinimumLength(1).WithMessage("Mã của sản phẩm phải có ít nhất 1 ký tự")
            .MaximumLength(50).WithMessage("Mã của sản phẩm không được vượt quá 50 ký tự");
        
        RuleFor(x => x.Name)
            .MinimumLength(1).WithMessage("Tên của sản phẩm phải có ít nhất 1 ký tự")
            .MaximumLength(200).WithMessage("Tên của sản phẩm không được vượt quá 200 ký tự");
        
        RuleFor(p => p.Description)
            .MinimumLength(1).WithMessage("Mô tả của sản phẩm phải có ít nhất 1 ký tự")
            .MaximumLength(1000).WithMessage("Mô tả của sản phẩm không được vượt quá 1000 ký tự");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Giá của sản phẩm phải lớn hơn 0");
        RuleFor(p => p.Sku)
            .MaximumLength(255).WithMessage("Mã SKU của sản phẩm không được vượt quá 255 ký tự");
    }
}