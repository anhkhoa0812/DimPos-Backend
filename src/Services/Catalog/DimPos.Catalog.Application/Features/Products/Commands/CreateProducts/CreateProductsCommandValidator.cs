using FluentValidation;

namespace DimPos.Catalog.Application.Features.Products.Commands.CreateProducts;

public class CreateProductsCommandValidator : AbstractValidator<CreateProductsCommand>
{
    public CreateProductsCommandValidator()
    {
        RuleFor(p => p.Code)
            .NotNull().WithMessage("Mã của sản phẩm không được bỏ trống")
            .NotEmpty().WithMessage("Mã của sản phẩm không được bỏ trống")
            .MinimumLength(1).WithMessage("Mã của sản phẩm phải có ít nhất 1 ký tự")
            .MaximumLength(50).WithMessage("Mã của sản phẩm không được vượt quá 50 ký tự");
        
        RuleFor(p => p.Name)
            .NotEmpty().WithMessage("Tên của sản phẩm không được bỏ trống")
            .MinimumLength(1).WithMessage("Tên của sản phẩm phải có ít nhất 1 ký tự")
            .MaximumLength(200).WithMessage("Tên của sản phẩm không được vượt quá 200 ký tự");
        
        RuleFor(p => p.Description)
            .NotEmpty().WithMessage("Mô tả của sản phẩm không được bỏ trống")
            .MinimumLength(1).WithMessage("Mô tả của sản phẩm phải có ít nhất 1 ký tự")
            .MaximumLength(1000).WithMessage("Mô tả của sản phẩm không được vượt quá 1000 ký tự");
        
        RuleFor(p => p.AlternativeCode)
            .MaximumLength(100).WithMessage("Mã thay thế của sản phẩm không được vượt quá 100 ký tự");
        
        RuleFor(p => p.IsAvailable)
            .NotEmpty().WithMessage("Trạng thái khả dụng của sản phẩm không được bỏ trống");
        
        RuleFor(p => p.SaleType)
            .NotEmpty().WithMessage("Loại hình bán hàng của sản phẩm không được bỏ trống");
    }
}

public class CreateProductVariantValidator : AbstractValidator<CreateProductVariant>
{
    public CreateProductVariantValidator()
    {
        RuleFor(p => p.Code)
            .NotEmpty().WithMessage("Mã của biến thể sản phẩm không được bỏ trống")
            .MinimumLength(1).WithMessage("Mã của biến thể sản phẩm phải có ít nhất 1 ký tự")
            .MaximumLength(50).WithMessage("Mã của biến thể sản phẩm không được vượt quá 50 ký tự");
        RuleFor(p => p.AlterativeCode)
            .MaximumLength(100).WithMessage("Mã thay thế của biến thể sản phẩm không được vượt quá 100 ký tự");
        RuleFor(p => p.Name)
            .NotEmpty().WithMessage("Tên của biến thể sản phẩm không được bỏ trống")
            .MinimumLength(1).WithMessage("Tên của biến thể sản phẩm phải có ít nhất 1 ký tự")
            .MaximumLength(200).WithMessage("Tên của biến thể sản phẩm không được vượt quá 200 ký tự");
        RuleFor(p => p.BrandPrice)
            .NotEmpty().WithMessage("Giá brand của biến thể sản phẩm không được bỏ trống");
    }
}
public class CreateProductVariantListValidator : AbstractValidator<List<CreateProductVariant>>
{
    public CreateProductVariantListValidator()
    {
        RuleForEach(x => x).SetValidator(new CreateProductVariantValidator());
    }
}