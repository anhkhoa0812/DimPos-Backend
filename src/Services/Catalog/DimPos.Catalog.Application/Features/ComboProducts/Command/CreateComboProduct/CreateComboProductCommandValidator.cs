using FluentValidation;

namespace DimPos.Catalog.Application.Features.ComboProducts.Command.CreateComboProduct;

public class CreateComboProductCommandValidator : AbstractValidator<CreateComboProductCommand>
{
    public CreateComboProductCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotNull().WithMessage("Mã của sản phẩm không được bỏ trống")
            .NotEmpty().WithMessage("Mã của sản phẩm không được bỏ trống")
            .MinimumLength(1).WithMessage("Mã của sản phẩm phải có ít nhất 1 ký tự")
            .MaximumLength(50).WithMessage("Mã của sản phẩm không được vượt quá 50 ký tự");
        
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên của sản phẩm không được bỏ trống")
            .MinimumLength(1).WithMessage("Tên của sản phẩm phải có ít nhất 1 ký tự")
            .MaximumLength(200).WithMessage("Tên của sản phẩm không được vượt quá 200 ký tự");
        
        RuleFor(p => p.Description)
            .MaximumLength(1000).WithMessage("Mô tả của sản phẩm không được vượt quá 1000 ký tự");
        
        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Giá của sản phẩm phải lớn hơn 0")
            .NotNull().WithMessage("Giá của sản phẩm không được bỏ trống")
            .NotEmpty().WithMessage("Giá của sản phẩm không được bỏ trống");
        RuleFor(p => p.Sku)
            .MaximumLength(255).WithMessage("Mã SKU của sản phẩm không được vượt quá 255 ký tự");
        
        RuleForEach(x => x.ProductImages).SetValidator(new CreateComboProductImagesValidator());
        RuleForEach(x => x.ItemProductVariants).SetValidator(new CreateItemProductVariantValidator());
    }
}
public class CreateComboProductImagesValidator : AbstractValidator<CreateComboProductImages>
{
    private static readonly string[] _allowedExtensions = new[]
    {
        ".jpeg", ".png", ".jpg", ".gif", ".bmp", ".webp"
    };
    public CreateComboProductImagesValidator()
    {
        RuleFor(x => x.Image)
            .Cascade(CascadeMode.Stop)
            .Must(file =>
            {
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

                return _allowedExtensions.Contains(extension);
            }).WithMessage("Chỉ các định dạng tệp .jpeg, .png, .jpg, .gif, .bmp, .webp  được phép tải lên.");
    }
}

public class CreateItemProductVariantValidator : AbstractValidator<CreateItemProductVariant>
{
    public CreateItemProductVariantValidator()
    {
        RuleFor(x => x.ProductVariantId)
            .NotEmpty().WithMessage("Id của biến thể sản phẩm không được bỏ trống")
            .NotNull().WithMessage("Id của biến thể sản phẩm không được bỏ trống")
            .NotEqual(Guid.Empty).WithMessage("Id của biến thể sản phẩm không được là Guid.Empty");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Số lượng của biến thể sản phẩm phải lớn hơn 0")
            .NotNull().WithMessage("Số lượng của biến thể sản phẩm không được bỏ trống")
            .NotEmpty().WithMessage("Số lượng của biến thể sản phẩm không được bỏ trống");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Thứ tự hiển thị phải lớn hơn hoặc bằng 0");
    }
}